using Microsoft.EntityFrameworkCore;
using Npgsql;
using Polly;
using Prisma.Domain.Common;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Infrastructure.Persistence.Repositories;
using Wolverine.EntityFrameworkCore;

namespace Prisma.Infrastructure.Persistence;

internal sealed class UnitOfWork(IDbContextOutbox<AppDbContext> outbox) : IUnitOfWork
{
    private readonly AppDbContext dbContext = outbox.DbContext;
    private readonly Dictionary<Type, object> _repositories = [];

    public IRepository<TEntity, TKey> GetOrCreateRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>
    {
        if (_repositories.TryGetValue(typeof(TEntity), out var repository))
        {
            return (IRepository<TEntity, TKey>)repository;
        }

        repository = new Repository<TEntity, TKey>(dbContext);
        _repositories.Add(typeof(TEntity), repository);
        return (IRepository<TEntity, TKey>)repository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        var pending = dbContext
            .ChangeTracker.Entries()
            .Count(e =>
                e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
            );
        try
        {
            await outbox.SaveChangesAndFlushMessagesAsync(ct); // scrapes + saves + commits + flushes
        }
        catch (DbUpdateException ex)
            when (ex.InnerException
                    is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }
            )
        {
            throw new UniqueConstraintViolationException(ex);
        }

        // The scraper publishes but never clears — clear so a second save in the same
        // request scope cannot re-broadcast the same SignalR message.
        foreach (var entry in dbContext.ChangeTracker.Entries<IDomainEventEnumerable>())
        {
            entry.Entity.ClearDomainEvents();
        }

        return pending;
    }
}
