using Microsoft.EntityFrameworkCore;
using Prisma.Domain.Common;
using Prisma.Domain.Interfaces;

namespace Prisma.Domain.Interfaces;

public interface IUnitOfWork
{
    IRepository<TEntity, TKey> GetOrCreateRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>;

    Task<int> SaveChangesAsync(CancellationToken ct);

    DbContext DbContext { get; }
}