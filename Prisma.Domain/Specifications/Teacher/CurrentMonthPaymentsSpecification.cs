using Ardalis.Specification;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Prisma.Domain.Specifications.Teacher;

public class CurrentMonthPaymentsSpecification : Specification<Payment>
{
    public CurrentMonthPaymentsSpecification(DateTimeOffset startOfMonth)
    {
        Query.Where(p => p.Status == PaymentStatus.Completed && p.PaidAt >= startOfMonth);
    }
}