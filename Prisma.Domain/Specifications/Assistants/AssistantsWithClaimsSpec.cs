using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Assistants;

public class AssistantsWithClaimsSpec : Specification<Assistant>
{
    public AssistantsWithClaimsSpec()
    {
        Query
            .Where(a => !a.IsDeleted)
            .Include(a => a.Claims)
            .AsNoTracking();
    }
}