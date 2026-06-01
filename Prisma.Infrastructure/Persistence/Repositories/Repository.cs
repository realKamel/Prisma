using Microsoft.EntityFrameworkCore;
using Prisma.Domain.Common;
using Prisma.Domain.Interfaces;

namespace Prisma.Infrastructure.Persistence.Repositories;

public class Repository<TEntity>(AppDbContext dbContext) :
    IRepository<TEntity> where TEntity : BaseEntity
{
    public async Task<ICollection<TEntity>> ListAsync(ISpecification<TEntity> spec,
        CancellationToken ct = default)
    {
        var query = SpecificationEvaluator
            .GetInstance
            .GetQuery(dbContext.Set<TEntity>(), spec);

        return await query.ToListAsync(ct);
    }

    public async Task<ICollection<TEntity>> ListAsync(CancellationToken ct = default)
    {
        return await dbContext.Set<TEntity>().ToListAsync(ct);
    }

    public async Task<TEntity?> GetBySpecAsync(ISpecification<TEntity> spec, CancellationToken ct = default)
    {
        var query = SpecificationEvaluator
            .GetInstance
            .GetQuery(dbContext.Set<TEntity>(), spec);
        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<int> CountAsync(ISpecification<TEntity> spec, CancellationToken ct = default)
    {
        var query = SpecificationEvaluator
            .GetInstance
            .GetQuery(dbContext.Set<TEntity>(), spec);
        return await query.CountAsync(ct);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await dbContext.Set<TEntity>().CountAsync(ct);
    }

    public async Task<bool> AnyAsync(ISpecification<TEntity> spec, CancellationToken ct = default)
    {
        var query = SpecificationEvaluator
            .GetInstance
            .GetQuery(dbContext.Set<TEntity>(), spec);
        return await query.AnyAsync(ct);
    }

    public async Task<bool> AnyAsync(CancellationToken ct = default)
    {
        return await dbContext.Set<TEntity>().AnyAsync(ct);
    }

    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        await dbContext.Set<TEntity>().AddAsync(entity, ct);
    }

    public void Update(TEntity entity, CancellationToken ct = default)
    {
        dbContext.Set<TEntity>().Update(entity);
    }

    public void Delete(TEntity entity, CancellationToken ct = default)
    {
        dbContext.Set<TEntity>().Remove(entity);
    }
}