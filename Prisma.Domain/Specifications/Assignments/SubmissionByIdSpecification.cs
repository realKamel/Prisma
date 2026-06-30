using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Assignments;

public class SubmissionByIdSpecification : Specification<AssignmentSubmission>
{
    public SubmissionByIdSpecification(int submissionId)
    {
        Query.Where(s => s.Id == submissionId);
    }
}