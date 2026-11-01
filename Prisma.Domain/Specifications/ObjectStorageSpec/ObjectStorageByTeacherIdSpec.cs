using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.ObjectStorageSpec;

public class ObjectStorageByTeacherId : Specification<ObjectStorage>
{
    public ObjectStorageByTeacherId(Guid teacherId, bool asNoTracking = false)
    {
        Query.Where(x => x.TeacherId == teacherId);
        if (asNoTracking)
        {
            Query.AsNoTracking();
        }
    }
}
