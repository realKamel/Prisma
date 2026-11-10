using Ardalis.Result;

namespace Prisma.Application.Abstractions.ObjectStorage;

public interface IStorageBucketService
{
    Task<Result> CreateBucketAsync(
        string bucketName,
        CancellationToken cancellationToken = default
    );

    Task<Result> DeleteBucketAsync(
        string bucketName,
        CancellationToken cancellationToken = default
    );
    // Task<Result> EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken = default);
}
