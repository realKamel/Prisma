using Ardalis.Specification;
using Prisma.Domain.Entities.PaymentAggregate;

namespace Prisma.Domain.Specifications.Payments;

public class PaymentByProviderRefSpec : Specification<Payment>
{
    public PaymentByProviderRefSpec(string providerRef)
    {
        Query.Where(p => p.ProviderRef == providerRef);
    }
}
