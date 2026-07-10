using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class StudentsByAcademicYearSpecification : Specification<Student>
{
    public StudentsByAcademicYearSpecification(int academicYearId)
    {
        Query.Where(s => s.AcademicYearId == academicYearId);
    }
}