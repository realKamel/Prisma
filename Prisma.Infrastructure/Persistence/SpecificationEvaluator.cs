using Microsoft.EntityFrameworkCore;
using Prisma.Domain.Common;
using Prisma.Domain.Entities;
using Prisma.Domain.Interfaces;

namespace Prisma.Infrastructure.Persistence;

public class SpecificationEvaluator
{
    public IQueryable<TEntity> GetQuery<TEntity>(
        IQueryable<TEntity> inputQuery,
        ISpecification<TEntity> specification) where TEntity : BaseEntity
    {
        var query = inputQuery;

        // 1. Apply criteria (filtering)
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        // 2. Apply includes
        query = specification.Includes.Aggregate(
            query,
            (current, include) => current.Include(include));

        query = specification.IncludeStrings.Aggregate(
            query,
            (current, include) => current.Include(include));


        // 3. Apply ordering
        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        var isOrdered = specification.OrderBy is not null
                        || specification.OrderByDescending is not null;

        // 4. Apply paging (MUST be last)
        if (specification.IsPagingEnabled)
        {
            if (!isOrdered)
            {
                query = query.OrderBy(e => e.Id);
            }

            query = query.Skip(specification.Skip).Take(specification.Take);
        }

        return query;
    }
}