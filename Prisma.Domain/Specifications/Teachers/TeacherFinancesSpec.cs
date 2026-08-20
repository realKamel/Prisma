using Ardalis.Specification;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Teachers;

public class TeacherFinancesSpec : Specification<Entities.PaymentAggregate.Payment>
{
    public TeacherFinancesSpec()
    {
        Query.Where(p => p.Status == PaymentStatus.Completed).AsNoTracking();
    }
}
