using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class StudentWithAcademicYearSpecification: Specification<Student, int?>
{
    public StudentWithAcademicYearSpecification(Guid studentId)
    {
        Query.Where(s => s.Id == studentId)
            .Select(x => x.AcademicYearId);
    }
}
