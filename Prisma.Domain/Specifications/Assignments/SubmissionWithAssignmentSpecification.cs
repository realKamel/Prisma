using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Prisma.Domain.Specifications.Assignments;

public class SubmissionWithAssignmentSpecification : Specification<AssignmentSubmission>
{
    public SubmissionWithAssignmentSpecification(int submissionId)
    {
        Query
            .Where(s => s.Id == submissionId)
            .Include(s => s.Assignment);
    }
}