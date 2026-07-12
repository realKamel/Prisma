using Ardalis.Specification;
using Prisma.Domain.Entities.PaymentAggregate;

namespace Prisma.Domain.Specifications.RedeemCodes;

public class CodeBatchWithLessonSpecification : Specification<RedeemCode>
{
    public CodeBatchWithLessonSpecification(int batchId)
    {
        Query.Where(b => b.Id == batchId)
            .Include(b => b.Lesson);
    }
}