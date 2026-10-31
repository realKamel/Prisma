using Ardalis.Result;
using Prisma.Domain.Common;

namespace Prisma.Domain.Entities.UserAggregate;

public sealed class ObjectStorage : BaseEntity<Guid>
{
    public Guid TeacherId { get; private set; }

    public StorageProvider Provider { get; private set; }

    public string BucketName { get; private set; } = null!;

    public StorageStatus Status { get; private set; }

    private ObjectStorage() { }

    public static Result<ObjectStorage> Create(Guid teacherId, string bucketName)
    {
        var result = new ObjectStorage()
        {
            Id = Guid.CreateVersion7(),
            TeacherId = teacherId,
            BucketName = bucketName,
            Status = StorageStatus.Pending,
        };
        return result;
    }

    public void MarkAsProvisioned(StorageProvider provider)
    {
        Provider = provider;
        Status = StorageStatus.Ready;
    }

    public void MarkAsFailed()
    {
        Status = StorageStatus.Failed;
    }

    public static string GenerateBucketName(Guid teacherId)
    {
        return $"prisma-teacher-{teacherId:N}";
    }
}
