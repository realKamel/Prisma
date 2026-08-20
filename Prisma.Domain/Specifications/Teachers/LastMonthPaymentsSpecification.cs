using Ardalis.Specification;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Teachers;

public class LastMonthPaymentsSpecification : Specification<Payment>
{
    public LastMonthPaymentsSpecification(
        DateTimeOffset startOfMonth,
        DateTimeOffset startOfLastMonth
    )
    {
        Query.Where(p =>
            p.Status == PaymentStatus.Completed
            && p.PaidAt >= startOfLastMonth
            && p.PaidAt < startOfMonth
        );
    }
}
