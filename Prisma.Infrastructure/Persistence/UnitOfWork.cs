using Prisma.Domain.Common;
using Prisma.Domain.Interfaces;
using Prisma.Infrastructure.Persistence.Repositories;

namespace Prisma.Infrastructure.Persistence;

public class UnitOfWork(AppDbContext dbContext, SpecificationEvaluator evaluator) : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = [];

    public IRepository<TEntity> GetOrCreateRepository<TEntity>() where TEntity : BaseEntity
    {
        if (_repositories.TryGetValue(typeof(TEntity), out object? repository))
        {
            return (IRepository<TEntity>)repository;
        }

        repository = new Repository<TEntity>(dbContext, evaluator);

        _repositories.Add(typeof(TEntity), repository);

        return (IRepository<TEntity>)repository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return await dbContext.SaveChangesAsync(ct);
    }
}