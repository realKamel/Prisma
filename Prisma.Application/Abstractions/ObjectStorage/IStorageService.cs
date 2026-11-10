namespace Prisma.Application.Abstractions.ObjectStorage;

public interface IStorageService
    : IStorageBucketService,
        IPresignedUrlService,
        IStorageObjectService
{
    string DefaultBucketName { get; }

    Task UploadFileAsync(
        string bucketName,
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default
    );

    Task UploadFileAsync(
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default
    );

    Task<string> GetDownloadUrlAsync(string bucketName, string objectKey, int expiryMinutes = 60);
    Task<string> GetDownloadUrlAsync(string objectKey, int expiryMinutes = 60);
}
