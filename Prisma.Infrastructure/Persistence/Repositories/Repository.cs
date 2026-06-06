using Microsoft.EntityFrameworkCore;
using Prisma.Domain.Common;
using Prisma.Domain.Interfaces;

namespace Prisma.Infrastructure.Persistence.Repositories;

public class Repository<TEntity>(AppDbContext dbContext, SpecificationEvaluator specificationEvaluator) :
    IRepository<TEntity> where TEntity : BaseEntity
{
    public async Task<ICollection<TEntity>> ListAsync(ISpecification<TEntity> spec,
        CancellationToken ct)
    {
        var query = specificationEvaluator.GetQuery(dbContext.Set<TEntity>(), spec);

        return await query.AsNoTrackingWithIdentityResolution().ToListAsync(ct);
    }

    public async Task<ICollection<TEntity>> ListAsync(CancellationToken ct)
    {
        return await dbContext.Set<TEntity>().AsNoTracking().ToListAsync(ct);
    }

    public async Task<TEntity?> GetBySpecAsync(ISpecification<TEntity> spec, CancellationToken ct,
        bool tracking = false)
    {
        var query = specificationEvaluator.GetQuery(dbContext.Set<TEntity>(), spec);

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<int> CountAsync(ISpecification<TEntity> spec, CancellationToken ct)
    {
        var query = specificationEvaluator.GetQuery(dbContext.Set<TEntity>(), spec);

        return await query.AsNoTracking().CountAsync(ct);
    }

    public async Task<int> CountAsync(CancellationToken ct)
    {
        return await dbContext.Set<TEntity>().AsNoTracking().CountAsync(ct);
    }

    public async Task<bool> AnyAsync(ISpecification<TEntity> spec, CancellationToken ct)
    {
        var query = specificationEvaluator.GetQuery(dbContext.Set<TEntity>(), spec);
        return await query.AsNoTracking().AnyAsync(ct);
    }

    public async Task<bool> AnyAsync(CancellationToken ct)
    {
        return await dbContext.Set<TEntity>().AsNoTracking().AnyAsync(ct);
    }

    public async Task AddAsync(TEntity entity, CancellationToken ct)
    {
        await dbContext.Set<TEntity>().AddAsync(entity, ct);
    }

    public void Update(TEntity entity)
    {
        dbContext.Set<TEntity>().Update(entity);
    }

    public void Delete(TEntity entity)
    {
        dbContext.Set<TEntity>().Remove(entity);
    }
}