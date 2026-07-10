using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Assignments;

public sealed class UngradedSubmissionsSpec : Specification<AssignmentSubmission>
{
    public UngradedSubmissionsSpec()
    {
        Query
            .Where(s => s.Score == null)
            .AsNoTracking();
    }
}