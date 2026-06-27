using Microsoft.EntityFrameworkCore;
using Prisma.Domain.Common;
using Prisma.Domain.Interfaces;
using Prisma.Infrastructure.Persistence.Repositories;

namespace Prisma.Infrastructure.Persistence;

public class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = [];

    public DbContext DbContext => dbContext;

    public IRepository<TEntity, TKey> GetOrCreateRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>
    {
        if (_repositories.TryGetValue(typeof(TEntity), out object? repository))
        {
            return (IRepository<TEntity, TKey>)repository;
        }

        repository = new Repository<TEntity, TKey>(dbContext);
        _repositories.Add(typeof(TEntity), repository);
        return (IRepository<TEntity, TKey>)repository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return await dbContext.SaveChangesAsync(ct);
    }
}