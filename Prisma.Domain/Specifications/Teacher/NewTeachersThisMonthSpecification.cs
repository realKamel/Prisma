using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Prisma.Domain.Specifications.Teacher;


public class NewTeachersThisMonthSpecification : Specification< Prisma.Domain.Entities.UserAggregate.Teacher>
{
    public NewTeachersThisMonthSpecification(DateTimeOffset startOfMonth)
    {
        Query.Where(t => t.CreatedAt >= startOfMonth);
    }
}