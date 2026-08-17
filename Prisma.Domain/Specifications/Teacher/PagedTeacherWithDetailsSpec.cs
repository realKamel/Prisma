using System.Linq.Expressions;
using Ardalis.Specification;

namespace Prisma.Domain.Specifications.Teacher;

public class PagedTeacherWithDetailsSpec<TSelector> : Specification<Entities.UserAggregate.Teacher, TSelector>
{
    public PagedTeacherWithDetailsSpec(string? search,
        Expression<Func<Entities.UserAggregate.Teacher, TSelector>> selector, int page, int pageSize)
    {
        Query
            .AsNoTrackingWithIdentityResolution();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchPattern = $"%{search}%";

            Query
                .Search(x => x.FirstName, searchPattern)
                .Search(x => x.SecondName, searchPattern)
                .Search(x => x.Subject, searchPattern);
        }

        var skipAmount = (page - 1) * pageSize;
        Query
            .Skip(skipAmount < 0 ? 0 : skipAmount)
            .Take(pageSize)
            .Select(selector);
    }
}