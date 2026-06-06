using Prisma.Domain.Entities;

namespace Prisma.Domain.Interfaces;

public interface IRepository<TEntity> where TEntity : BaseEntity
{
    Task<ICollection<TEntity>> ListAsync(ISpecification<TEntity> spec, CancellationToken ct);
    Task<ICollection<TEntity>> ListAsync(CancellationToken ct);
    Task<TEntity?> GetBySpecAsync(ISpecification<TEntity> spec, CancellationToken ct, bool tracking = false);
    Task<int> CountAsync(ISpecification<TEntity> spec, CancellationToken ct);
    Task<int> CountAsync(CancellationToken ct);
    Task<bool> AnyAsync(ISpecification<TEntity> spec, CancellationToken ct);
    Task<bool> AnyAsync(CancellationToken ct);
    Task AddAsync(TEntity entity, CancellationToken ct);
    void Update(TEntity entity);
    void Delete(TEntity entity);
}