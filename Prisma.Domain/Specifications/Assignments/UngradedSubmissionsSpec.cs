using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Assignments;

public sealed class UngradedSubmissionsSpec : Specification<AssignmentSubmission>
{
    public UngradedSubmissionsSpec(Guid teacherId)
    {
        Query
            .Where(s => s.Score == null
                     && s.Assignment!.Lesson!.TeacherId == teacherId)
            .AsNoTracking();
    }
}