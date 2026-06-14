using Ardalis.Specification;
using Prisma.Domain.Common;

namespace Prisma.Domain.Interfaces;

public interface IRepository<TEntity, TKey> : IRepositoryBase<TEntity> where TEntity : class, IEntity<TKey>
{
    TEntity Add(TEntity entity);
    IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities);
    void Update(TEntity entity);
    void UpdateRange(IEnumerable<TEntity> entities);
    void Delete(TEntity entity);
    void DeleteRange(IEnumerable<TEntity> entities);
}