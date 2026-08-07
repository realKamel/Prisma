using Ardalis.Specification;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.Quizzes.Specifications;

public class StudentsByAcademicYearSpecification
    : Specification<Student, StudentListProjection>
{
    public StudentsByAcademicYearSpecification(int academicYearId)
    {
        Query
            .Where(s => s.AcademicYearId == academicYearId)

            .Select(s => new StudentListProjection
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName
            });
    }
}