using Ardalis.Specification;

namespace Prisma.Domain.Specifications.Teachers;

public class TeacherByIdSpecification : Specification<Entities.UserAggregate.Teacher>
{
    public TeacherByIdSpecification(Guid teacherId)
    {
        Query.Where(t => t.Id == teacherId);
    }
}
