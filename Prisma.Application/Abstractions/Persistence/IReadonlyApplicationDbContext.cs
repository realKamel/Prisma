using Prisma.Domain.Common;

namespace Prisma.Application.Abstractions.Persistence;

public interface IReadonlyAppDbContext
{
    IQueryable<TEntity> Entity<TEntity, TKey>()
        where TEntity : BaseEntity<TKey>;
}
