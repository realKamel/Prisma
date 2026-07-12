using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class AllAcademicYearsSpecification: Specification<AcademicYear>
{
    public AllAcademicYearsSpecification()
    {
        Query.OrderBy(a => a.Id);
    }
}
