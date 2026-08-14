using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Teacher;


public class TeacherFinancesSpec : Specification<Prisma.Domain.Entities.PaymentAggregate.Payment>
{
    public TeacherFinancesSpec()
    {
        Query.Where(p => p.Status == PaymentStatus.Completed)
             .AsNoTracking();
    }
}