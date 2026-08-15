using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.PaymentAggregate;

namespace Prisma.Domain.Specifications.RedeemCodes;

public class CodeBatchWithProjectionSpec<TResult> : Specification<RedeemCode, TResult>
{
    public CodeBatchWithProjectionSpec(int batchId, Expression<Func<RedeemCode, TResult>> projection)
    {
        Query
            .Where(b => b.Id == batchId)
            .AsNoTracking()
            .Select(projection);
    }
}