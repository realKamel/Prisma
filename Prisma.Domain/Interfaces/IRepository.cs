using Prisma.Domain.Common;

namespace Prisma.Domain.Interfaces;

public interface IRepository<TEntity> where TEntity : BaseEntity
{
    Task<ICollection<TEntity>> ListAsync(ISpecification<TEntity> spec, CancellationToken ct = default);
    Task<ICollection<TEntity>> ListAsync(CancellationToken ct = default);
    Task<TEntity?> GetBySpecAsync(ISpecification<TEntity> spec, CancellationToken ct = default);
    Task<int> CountAsync(ISpecification<TEntity> spec, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task<bool> AnyAsync(ISpecification<TEntity> spec, CancellationToken ct = default);
    Task<bool> AnyAsync(CancellationToken ct = default);
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    void Update(TEntity entity, CancellationToken ct = default);
    void Delete(TEntity entity, CancellationToken ct = default);
}