using Ardalis.Specification;
using Prisma.Domain.Entities.PaymentAggregate;

namespace Prisma.Domain.Specifications.RedeemCodes;

public class CodeBatchByIdSpecification : Specification<RedeemCode>
{
    public CodeBatchByIdSpecification(int batchId, Guid teacherId)
    {
        Query.Where(b => b.Id == batchId && b.CreatedByTeacherId == teacherId);

        Query.Include(b => b.Lesson)
            .Include(b => b.AcademicYear)
            .Include(b => b.GeneratedCodes)
                .ThenInclude(c => c.RedeemedByStudent);
    }
}