using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.VideoSpecs;

public sealed class VideoByProviderId : Specification<Video>
{
    public VideoByProviderId(long libraryId, Guid providerVideoId, bool asNoTracking = false)
    {
        Query.Where(x =>
            x.ProviderVideoLibraryId == libraryId && x.ProviderVideoId == providerVideoId
        );

        if (asNoTracking)
        {
            Query.AsNoTracking();
        }
    }
}
