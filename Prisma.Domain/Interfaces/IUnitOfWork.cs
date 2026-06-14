using Prisma.Domain.Common;

namespace Prisma.Domain.Interfaces;

public interface IUnitOfWork
{
    public IRepository<TEntity, TKey> GetOrCreateRepository<TEntity, TKey>() where TEntity : class, IEntity<TKey>;
    public Task<int> SaveChangesAsync(CancellationToken ct = default);
}