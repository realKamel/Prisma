using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.VideoSpecs;

public sealed class VideoByIdWithProjection<TResult> : Specification<Video, TResult>
{
    public VideoByIdWithProjection(
        int videoId,
        Expression<Func<Video, TResult>> expression,
        bool tracking = false
    )
    {
        // Scoping by the owning library keeps a caller from re-signing another workspace's video.
        Query.Where(x => x.Id == videoId).AsNoTracking().Include(x => x.VideoLibrary);

        if (!tracking)
        {
            Query.AsNoTracking();
        }

        Query.Select(expression);
    }
}

public sealed class VideoByIdWithVideoLibrary : Specification<Video>
{
    public VideoByIdWithVideoLibrary(int videoId, bool asNoTracking = true)
    {
        Query.Where(x => x.Id == videoId).Include(x => x.VideoLibrary);

        if (asNoTracking)
        {
            Query.AsNoTracking();
        }
    }
}
