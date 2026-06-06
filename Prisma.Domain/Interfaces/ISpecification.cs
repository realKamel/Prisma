using System.Linq.Expressions;
using Prisma.Domain.Common;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Interfaces;

public interface ISpecification<TEntity> where TEntity : BaseEntity
{
    // Filtering
    Expression<Func<TEntity, bool>>? Criteria { get; }

    // Navigation properties
    List<Expression<Func<TEntity, object>>> Includes { get; }
    List<string> IncludeStrings { get; }

    // Ordering
    Expression<Func<TEntity, object>>? OrderBy { get; }
    Expression<Func<TEntity, object>>? OrderByDescending { get; }

    // Grouping
    Expression<Func<TEntity, object>>? GroupBy { get; }

    // Paging
    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }
}