using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Assignments;

public class SubmissionDetailSpecification : Specification<AssignmentSubmission>
{
    public SubmissionDetailSpecification(int submissionId)
    {
        Query
            .Where(s => s.Id == submissionId)
            .Include(s => s.Student)
            .Include(s => s.Assignment)
                .ThenInclude(a => a.Lesson);
    }
}