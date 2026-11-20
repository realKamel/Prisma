using Ardalis.Specification;
using Prisma.Domain.Aggregates.VideoLibraryAggregate;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.VideoLibrarySpecs;

public sealed class VideoLibraryByProviderLibraryId : Specification<VideoLibrary>
{
    public VideoLibraryByProviderLibraryId(long providerLibraryId, bool asNoTracking = false)
    {
        Query.Where(x => x.ProviderLibraryId == providerLibraryId).Include(x => x.Videos);

        if (asNoTracking)
        {
            Query.AsNoTracking();
        }
    }
}
