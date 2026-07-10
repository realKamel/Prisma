using Prisma.Domain.Common;

namespace Prisma.Domain.Interfaces;

public interface IUnitOfWork
{
    IRepository<TEntity, TKey> GetOrCreateRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>;

    Task<int> SaveChangesAsync(CancellationToken ct);
}