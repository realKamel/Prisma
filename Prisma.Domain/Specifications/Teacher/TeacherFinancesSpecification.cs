using Ardalis.Specification;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Teachers;

public class TeacherFinancesSpecification : Specification<Payment>
{
    public TeacherFinancesSpecification()
    {
        Query.Where(p => p.Status == PaymentStatus.Completed)
             .Include(p => p.Student)
             .Include(p => p.Lesson)
             .OrderByDescending(p => p.PaidAt);
    }
}