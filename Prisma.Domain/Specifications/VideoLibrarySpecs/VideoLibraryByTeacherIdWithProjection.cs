using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.VideoLibrarySpecs;

public sealed class VideoLibraryByTeacherIdWithProjection<TResult>
    : Specification<VideoLibrary, TResult>
{
    public VideoLibraryByTeacherIdWithProjection(
        Guid teacherId,
        Expression<Func<VideoLibrary, TResult>> expression,
        bool asNoTracking = false
    )
    {
        Query.Where(x => x.TeacherId == teacherId);

        if (asNoTracking)
        {
            Query.AsNoTracking();
        }

        Query.Select(expression);
    }
}
