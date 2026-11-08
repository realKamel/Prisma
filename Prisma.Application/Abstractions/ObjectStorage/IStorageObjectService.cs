using Ardalis.Result;

namespace Prisma.Application.Abstractions.ObjectStorage;

public interface IStorageObjectService
{
    // string GetPublicUrl(string bucketName, string objectKey);
    // Task SetPublicReadPolicyAsync(string bucketName, params string[] publicPrefixes);
    Task DeleteFileAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken = default
    );

    Task DeleteFileAsync(string objectKey, CancellationToken cancellationToken = default);

    // Task<Result> DeleteObjectsAsync(
    //     string bucketName,
    //     IEnumerable<string> objectKeys,
    //     CancellationToken cancellationToken = default
    // );

    Task<Result> DeleteObjectAsync(
        string bucketName,
        string objectKeys,
        CancellationToken cancellationToken = default
    );

    Task<Stream> GetObjectAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken = default
    );

    Task<Result<FileObjectMetadata>> GetObjectMetadataAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken = default
    );
}
