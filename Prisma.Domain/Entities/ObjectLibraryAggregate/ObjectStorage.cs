using Ardalis.Result;
using Prisma.Domain.Common;
using Prisma.Domain.DomainEvents.ObjectStorage;

namespace Prisma.Domain.Entities.ObjectLibraryAggregate;

public sealed class ObjectStorage : BaseEntity<Guid>
{
    public Guid TeacherId { get; private set; }

    public StorageProvider Provider { get; private set; }

    public string BucketName { get; private set; }

    public StorageStatus Status { get; private set; }

    public ICollection<FileObject> Files { get; private set; } = [];

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
        return $"wasla-teacher-{teacherId:N}";
    }

    /// <summary>
    /// Method to add a new file object to this storage bucket,
    /// enforcing business rules (e.g., storage must be ready).
    /// </summary>
    public Result<FileObject> AddFile(
        Guid fileId,
        string objectKey,
        string fileName,
        string contentType,
        long size
    )
    {
        // 1. Enforce aggregate invariants / business rules
        if (Status != StorageStatus.Ready)
        {
            return Result.Invalid(
                new ValidationError(
                    "StorageStatus",
                    "Cannot attach files to storage that is not in a Ready state."
                )
            );
        }

        // 2. Create the child entity using its factory method
        var fileObjectResult = FileObject.CreateWithCustomKey(
            fileId,
            this.Id, // ObjectStorageId
            // this.BucketName, // ObjectStorage bucket name
            objectKey,
            fileName,
            contentType,
            size
        );

        if (!fileObjectResult.IsSuccess)
        {
            return fileObjectResult.Map();
        }

        // 3. Add to the aggregate collection safely
        Files.Add(fileObjectResult.Value);

        return Result.Success(fileObjectResult.Value);
    }

    public Result RemoveFile(Guid fileId)
    {
        if (Status != StorageStatus.Ready)
        {
            return Result.Invalid(
                new ValidationError(
                    nameof(Status),
                    "Cannot remove files from storage that is not in a Ready state."
                )
            );
        }

        var file = Files.FirstOrDefault(x => x.Id == fileId);

        if (file is null)
        {
            return Result.Success();
        }

        Files.Remove(file);

        AddDomainEvent(new FileObjectRemoved(Id, fileId));

        return Result.Success();
    }
}
