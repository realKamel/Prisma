using Ardalis.Specification;
using Prisma.Domain.Entities.PaymentAggregate;

namespace Prisma.Domain.Specifications.RedeemCodes;

public class GeneratedCodeByValueSpecification : Specification<GeneratedCode>
{
    public GeneratedCodeByValueSpecification(string code)
    {
        // Normalize: uppercase, strip spaces and dashes to be forgiving
        var normalized = code.ToUpperInvariant().Replace("-", "").Replace(" ", "");

        // Match either exact or normalized form
        Query.Where(c =>
            c.Code.ToUpper() == code.ToUpperInvariant() ||
            c.Code.ToUpper().Replace("-", "") == normalized);
    }
}