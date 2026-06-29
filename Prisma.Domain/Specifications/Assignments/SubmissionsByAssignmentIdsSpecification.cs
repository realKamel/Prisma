using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Assignments;

public class SubmissionsByAssignmentIdsSpecification : Specification<AssignmentSubmission>
{
    public SubmissionsByAssignmentIdsSpecification(List<int> assignmentIds)
    {
        Query
            .Where(s => assignmentIds.Contains(s.AssignmentId))
            .Include(s => s.Student);
    }
}