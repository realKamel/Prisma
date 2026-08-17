using Ardalis.Specification;
using TeacherEntity = Prisma.Domain.Entities.UserAggregate.Teacher;

namespace Prisma.Domain.Specifications.Teacher;

public class TeacherWithDetailsSpecification : Specification<TeacherEntity>
{
    public TeacherWithDetailsSpecification()
    {
        Query.Include(t => t.TeacherStudents)
            .Include(t => t.Lessons);
    }
}