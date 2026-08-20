using Ardalis.Specification;

namespace Prisma.Domain.Specifications.Teachers;

public class NewTeachersThisMonthSpecification : Specification<Entities.UserAggregate.Teacher>
{
    public NewTeachersThisMonthSpecification(DateTimeOffset startOfMonth)
    {
        Query.Where(t => t.CreatedAt >= startOfMonth);
    }
}
