using Ardalis.Specification;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Teachers;

public class ActiveTeachersSpecification : Specification<Entities.UserAggregate.Teacher>
{
    public ActiveTeachersSpecification()
    {
        Query.Where(t => t.Status == TeacherStatus.Active);
    }
}
