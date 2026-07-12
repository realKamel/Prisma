using Ardalis.Specification;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Specifications.Teacher;

public class TeacherPreferencesByTeacherIdSpecification : Specification<TeacherPreferences>
{
    public TeacherPreferencesByTeacherIdSpecification(Guid teacherId)
    {
        Query.Where(p => p.Id == teacherId);
    }
}

