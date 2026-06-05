using Prisma.Domain.Entities;

namespace Prisma.Domain.Interfaces;

public interface IUnitOfWork
{
    public IRepository<TEntity> GetOrCreateRepository<TEntity>() where TEntity : BaseEntity;
    public Task<int> SaveChangesAsync(CancellationToken ct = default);
}