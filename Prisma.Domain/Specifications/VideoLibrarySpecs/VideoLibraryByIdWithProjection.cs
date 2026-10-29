using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.VideoLibrarySpecs;

public sealed class VideoLibraryByIdWithProjection<TResult> : Specification<VideoLibrary, TResult>
{
    public VideoLibraryByIdWithProjection(
        Guid teacherId,
        Expression<Func<VideoLibrary, TResult>> expression
    )
    {
        Query.AsNoTracking().Where(x => x.TeacherId == teacherId).Select(expression);
    }
}
