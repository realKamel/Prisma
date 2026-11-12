using Ardalis.Specification;
using Prisma.Domain.Entities.ObjectLibraryAggregate;

namespace Prisma.Domain.Specifications.ObjectStorageSpec;

public sealed class ObjectStorageByWorkspaceIdSpec : Specification<ObjectStorage>
{
    public ObjectStorageByWorkspaceIdSpec(Guid workspaceId)
    {
        Query.Where(x => x.TeacherId == workspaceId);
    }
}
