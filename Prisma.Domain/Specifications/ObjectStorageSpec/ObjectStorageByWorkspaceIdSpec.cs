using Ardalis.Specification;
using Prisma.Domain.Aggregates.ObjectLibraryAggregate;

namespace Prisma.Domain.Specifications.ObjectStorageSpec;

public sealed class ObjectStorageByWorkspaceIdSpec : Specification<ObjectStorage>
{
    public ObjectStorageByWorkspaceIdSpec(Guid workspaceId)
    {
        Query.Where(x => x.TeacherId == workspaceId).Include(x => x.Files);
    }
}
