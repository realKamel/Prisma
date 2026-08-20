using Ardalis.Specification;

namespace Prisma.Domain.Specifications.Teachers;

public class FilteredTeacherSpec : Specification<Entities.UserAggregate.Teacher>
{
    public FilteredTeacherSpec(string? search)
    {
        Query.AsNoTrackingWithIdentityResolution();

        if (string.IsNullOrWhiteSpace(search))
            return;

        var searchPattern = $"%{search}%";

        Query
            .Search(x => x.FirstName, searchPattern)
            .Search(x => x.SecondName, searchPattern)
            .Search(x => x.Subject, searchPattern);
    }
}
