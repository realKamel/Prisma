
using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class IncompleteEnrollmentsSpecification : Specification<Enrollment>
{
    public IncompleteEnrollmentsSpecification(List<Guid> studentIds, IReadOnlyCollection<int> lessonIds)
    {
        Query.Where(e => !e.IsCompleted
                      && studentIds != null && studentIds.Contains(e.StudentId.Value)
                      && lessonIds != null && lessonIds.Contains(e.LessonId.Value));
    }
}