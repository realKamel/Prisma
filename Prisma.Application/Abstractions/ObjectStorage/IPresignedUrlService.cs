using Ardalis.Result;

namespace Prisma.Application.Abstractions.ObjectStorage;

public interface IPresignedUrlService
{
    Task<Result<string>> GetPreSignedUploadUrlAsync(
        string objectKey,
        long expiresInSeconds,
        string? contentType = null,
        CancellationToken cancellationToken = default
    );

    Task<Result<string>> GetPreSignedUploadUrlAsync(
        string bucket,
        string objectKey,
        long expiresInSeconds,
        string? contentType = null,
        CancellationToken cancellationToken = default
    );

    Task<Result<string>> GetPreSignedDownloadUrlAsync(
        string objectKey,
        long expiresInSeconds,
        CancellationToken cancellationToken = default
    );

    Task<Result<string>> GetPreSignedDownloadUrlAsync(
        string bucket,
        string objectKey,
        long expiresInSeconds,
        CancellationToken cancellationToken = default
    );
}
