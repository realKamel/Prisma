using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.VideoLibrarySpecs;

public sealed class VideoLibraryByTeacherId : Specification<VideoLibrary>
{
    public VideoLibraryByTeacherId(Guid teacherId, bool asNoTracking = false)
    {
        Query.Where(x => x.TeacherId == teacherId);

        if (asNoTracking)
        {
            Query.AsNoTracking();
        }
    }
}
