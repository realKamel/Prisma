using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Prisma.Domain.Specifications.Teacher;

public class ActiveTeachersSpecification : Specification<Prisma.Domain.Entities.UserAggregate.Teacher>
{
    public ActiveTeachersSpecification()
    {
        Query.Where(t => t.Status == TeacherStatus.Active);
    }
}