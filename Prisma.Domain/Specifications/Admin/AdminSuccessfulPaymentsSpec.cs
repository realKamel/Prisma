using Ardalis.Specification;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Admin;

public sealed class AdminSuccessfulPaymentsSpec : Specification<Payment, PaymentActivityProjection>
{
    public AdminSuccessfulPaymentsSpec()
    {
        Query
            .Where(p => p.Status == PaymentStatus.Completed)
            .OrderByDescending(p => p.PaidAt ?? p.CreatedAt)
            .Take(5)
            .Select(p => new PaymentActivityProjection(
                p.Id,
                p.StudentId,
                p.Amount,
                p.Currency,
                p.Provider,
                p.ProviderRef,
                p.PaidAt,
                p.CreatedAt
            ));
    }
}
public sealed record PaymentActivityProjection(
    int Id,
    Guid StudentId,
    decimal Amount,
    string Currency,
    string Provider,
    string ProviderRef,
    DateTimeOffset? PaidAt,
    DateTimeOffset? CreatedAt
);