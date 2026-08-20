using Ardalis.Specification;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Teachers;

public class CurrentMonthPaymentsSpecification : Specification<Payment>
{
    public CurrentMonthPaymentsSpecification(DateTimeOffset startOfMonth)
    {
        Query.Where(p => p.Status == PaymentStatus.Completed && p.PaidAt >= startOfMonth);
    }
}
