using Ardalis.Specification;
using Prisma.Domain.Entities.ObjectLibraryAggregate;

namespace Prisma.Domain.Specifications.ObjectStorageSpec;

public sealed class ObjectStorageByTeacherId : Specification<ObjectStorage>
{
    public ObjectStorageByTeacherId(Guid teacherId, bool asNoTracking = false)
    {
        Query.Where(x => x.TeacherId == teacherId).Include(x => x.Files);

        if (asNoTracking)
        {
            Query.AsNoTracking();
        }
    }
}
