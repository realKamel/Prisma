using Microsoft.EntityFrameworkCore;
using Npgsql;
using Polly;
using Prisma.Domain.Common;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Infrastructure.Persistence.Repositories;

namespace Prisma.Infrastructure.Persistence;

public class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
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

        try
        {
            return await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new UniqueConstraintViolationException(ex);
        }
    }
}