using Ardalis.Specification;
using Prisma.Domain.Entities.ObjectLibraryAggregate;

namespace Prisma.Domain.Specifications.ObjectStorageSpec;

public sealed class ObjectStorageByFileObjectIdSpec : Specification<ObjectStorage>
{
    public ObjectStorageByFileObjectIdSpec(Guid fileObjectId)
    {
        Query.Where(x => x.Files.Any(f => f.Id == fileObjectId)).Include(x => x.Files);
    }
}
