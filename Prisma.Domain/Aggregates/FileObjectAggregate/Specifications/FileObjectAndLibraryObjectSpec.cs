using Ardalis.Specification;

namespace Prisma.Domain.Aggregates.FileObjectAggregate.Specifications;

public sealed class FileObjectAndLibraryObjectSpec : Specification<FileObject>
{
    public FileObjectAndLibraryObjectSpec(Guid objectId)
    {
        Query.Where(f => f.Id == objectId).Include(f => f.ObjectStorage);
    }
}
