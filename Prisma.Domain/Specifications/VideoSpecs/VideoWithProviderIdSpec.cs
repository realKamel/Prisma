using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.VideoSpecs;

public sealed class VideoWithProviderIdSpec : Specification<Video>
{
    public VideoWithProviderIdSpec(Guid providerVideoId, int videoLibraryId)
    {
        // Scoping by the owning library keeps a caller from re-signing another workspace's video.
        Query
            .Where(x => x.ProviderVideoId == providerVideoId && x.VideoLibraryId == videoLibraryId)
            .AsNoTracking()
            .Include(x => x.VideoLibrary);
    }
}
