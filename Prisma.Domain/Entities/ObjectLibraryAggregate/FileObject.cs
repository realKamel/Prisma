using Ardalis.Result;
using Prisma.Domain.Common;
using Prisma.Domain.DomainEvents.ObjectStorage;

namespace Prisma.Domain.Entities.ObjectLibraryAggregate;

public sealed class FileObject : BaseEntity<Guid>
{
    private FileObject() { } // EF Core

    private FileObject(
        Guid id,
        Guid objectStorageId,
        string objectKey,
        string fileName,
        string contentType,
        long size
    )
    {
        Id = id;
        ObjectStorageId = objectStorageId;
        ObjectKey = objectKey;
        FileName = fileName;
        ContentType = contentType;
        Size = size;
        FileStatus = FileStatus.PendingUpload;
    }

    // Storage identity
    public string ObjectKey { get; private set; }

    // File metadata
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public long Size { get; private set; }

    // Scanning lifecycle
    public FileStatus FileStatus { get; private set; }

    public int ScanAttempts { get; private set; }
    public DateTimeOffset? ScanStartedAt { get; private set; }
    public DateTimeOffset? ScannedAt { get; private set; }

    // Optional audit information
    public string? ScanFailureReason { get; private set; }
    public string? ContentHash { get; private set; }

    public ObjectStorage ObjectStorage { get; private set; }
    public Guid ObjectStorageId { get; private set; }

    public static Result<FileObject> Create(
        Guid objectStorageId,
        string bucketName,
        string objectKey,
        string fileName,
        string contentType,
        long size
    )
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(bucketName))
        {
            errors.Add(
                new ValidationError("BucketName", "Bucket name cannot be null or whitespace.")
            );
        }

        if (string.IsNullOrWhiteSpace(objectKey))
        {
            errors.Add(
                new ValidationError("ObjectKey", "Object key cannot be null or whitespace.")
            );
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            errors.Add(new ValidationError("FileName", "File name cannot be null or whitespace."));
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            errors.Add(
                new ValidationError("ContentType", "Content type cannot be null or whitespace.")
            );
        }

        if (size <= 0)
        {
            errors.Add(new ValidationError("Size", "File size must be greater than zero."));
        }

        if (objectStorageId == Guid.Empty)
        {
            errors.Add(
                new ValidationError("ObjectStorageId", "Object storage ID cannot be empty.")
            );
        }

        if (errors.Count > 0)
        {
            return Result.Invalid(errors);
        }

        return new FileObject(
            Guid.CreateVersion7(),
            objectStorageId,
            objectKey,
            fileName,
            contentType,
            size
        );
    }

    private void ChangeStatus(FileStatus target)
    {
        if (!FileStatus.CanTransitionTo(target))
        {
            throw new InvalidOperationException(
                $"Cannot transition file {Id} from {FileStatus.Name} to {target.Name}."
            );
        }

        FileStatus = target;
    }

    public void MarkUploadCompleted()
    {
        ChangeStatus(FileStatus.PendingScan);
        AddDomainEvent(new FileObjectUploaded(ObjectStorageId, Id));
    }

    public void MarkAsAvailable(DateTimeOffset scannedAt, string? contentHash = null)
    {
        ChangeStatus(FileStatus.Available);

        ScannedAt = scannedAt;
        ScanStartedAt = null;
        ContentHash = contentHash;
        ScanFailureReason = null;
    }

    public void MarkAsRejected(DateTimeOffset scannedAt, string? reason = null)
    {
        ChangeStatus(FileStatus.Rejected);

        ScannedAt = scannedAt;
        ScanStartedAt = null;
        ScanFailureReason = reason;
    }

    public void MarkScanAsFailed(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        ChangeStatus(FileStatus.ScanFailed);

        ScanStartedAt = null;
        ScanFailureReason = reason;
    }

    public void MarkScanAsFailed()
    {
        ChangeStatus(FileStatus.ScanFailed);
    }

    public void RetryScan()
    {
        ChangeStatus(FileStatus.PendingScan);

        ScanStartedAt = null;
        ScannedAt = null;
        ScanFailureReason = null;
    }

    public void MarkAsScanning(DateTimeOffset startedAt)
    {
        ChangeStatus(FileStatus.Scanning);

        ScanStartedAt = startedAt;
        ScanAttempts++;
        ScanFailureReason = null;
    }
}
