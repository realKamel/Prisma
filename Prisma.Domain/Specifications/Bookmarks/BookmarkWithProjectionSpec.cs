using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications;

public sealed class BookmarkWithProjectionSpec<TResult> : Specification<Bookmark, TResult>
{
    public BookmarkWithProjectionSpec(
        Guid studentId,
        int sectionId,
        Expression<Func<Bookmark, TResult>> projection
    )
    {
        Query
            .Where(b => b.StudentId == studentId && b.SectionId == sectionId)
            .OrderBy(b => b.PositionSeconds)
            .AsNoTracking()
            .Select(projection);
    }
}