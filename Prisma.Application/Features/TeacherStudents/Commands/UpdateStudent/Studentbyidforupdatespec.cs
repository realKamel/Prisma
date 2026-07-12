using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.TeacherStudents.Commands.UpdateStudent;

public class StudentByIdForUpdateSpec : Specification<Student>
{
    public StudentByIdForUpdateSpec(Guid studentId, Guid teacherId)
    {
        Query.Where(s => s.Id == studentId && s.TeacherId == teacherId);
    }
}