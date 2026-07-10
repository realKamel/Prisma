using Ardalis.Specification;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Admin;

public sealed class AdminSuccessfulPaymentsSpec : Specification<Payment>
{
    public AdminSuccessfulPaymentsSpec()
    {
        Query
            .Where(p => p.Status == PaymentStatus.Completed)
            .Include(p => p.Student)
            .Include(p => p.Lesson);
    }
}