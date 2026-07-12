using Ardalis.Specification.EntityFrameworkCore;
using Prisma.Domain.Common;
using Prisma.Domain.Interfaces;

namespace Prisma.Infrastructure.Persistence.Repositories;

public class Repository<TEntity, TKey>(AppDbContext dbContext)
    : RepositoryBase<TEntity>(dbContext), IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    public TEntity Add(TEntity entity)
    {
        dbContext.Set<TEntity>().Add(entity);
        return entity;
    }

    public IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities)
    {
        var range = entities.ToList();
        dbContext.Set<TEntity>().AddRange(range);
        return range;
    }

    public void Update(TEntity entity)
    {
        dbContext.Set<TEntity>().Update(entity);
    }

    public void UpdateRange(IEnumerable<TEntity> entities)
    {
        var range = entities.ToList();
        dbContext.Set<TEntity>().UpdateRange(range);
    }

    public void Delete(TEntity entity)
    {
        dbContext.Set<TEntity>().Remove(entity);
    }

    public void DeleteRange(IEnumerable<TEntity> entities)
    {
        var range = entities.ToList();
        dbContext.Set<TEntity>().RemoveRange(range);
    }

    // just to enforce UoW
    public sealed override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}