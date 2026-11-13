using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Ardalis.Result;
using Microsoft.Extensions.Logging;
using Prisma.Application.Abstractions.ObjectStorage;

namespace Prisma.Infrastructure.Services.ObjectStorageServices;

public sealed partial class R2StorageService(IAmazonS3 s3, ILogger<R2StorageService> logger)
    : IStorageService
{
    private const long MaxExpirySeconds = 604_800;
    private const int MaxExpiryMinutes = 10_080;
    private const string MetadataPrefix = "x-amz-meta-";
    public string DefaultBucketName { get; } = "wasla";

    public async Task UploadFileAsync(
        string bucketName,
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bucketName);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectKey);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            InputStream = content,
            ContentType = contentType,
        };

        try
        {
            await s3.PutObjectAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (AmazonS3Exception ex)
        {
            LogUploadFailed(bucketName, objectKey, ex.ErrorCode, ex);
            throw;
        }
        catch (Exception ex)
        {
            LogUnexpectedUploadFailure(bucketName, objectKey, ex);
            throw;
        }
    }

    public Task UploadFileAsync(
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default
    )
    {
        return UploadFileAsync(
            DefaultBucketName,
            objectKey,
            content,
            contentType,
            cancellationToken
        );
    }

    #region Buckets

    public async Task<Result> CreateBucketAsync(
        string bucketName,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bucketName);

        try
        {
            var request = new PutBucketRequest { BucketName = bucketName };

            await s3.PutBucketAsync(request, cancellationToken);

            LogBucketCreated(logger, bucketName);

            return Result.Success();
        }
        catch (AmazonS3Exception ex)
        {
            LogBucketCreationFailed(logger, ex, bucketName, ex.ErrorCode);

            return ex.ErrorCode switch
            {
                "BucketAlreadyOwnedByYou" => Result.Success(),

                "BucketAlreadyExists" => Result.Conflict(
                    $"The bucket '{bucketName}' already exists and is owned by another account"
                ),

                "AccessDenied" => Result.Forbidden(
                    "The storage provider denied permission to create the bucket"
                ),

                _ => Result.Error("Failed to create the storage bucket"),
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            UnexpectedErrorCreatingBucket(logger, bucketName, ex);

            return Result.Error("An unexpected error occurred while creating the bucket.");
        }
    }

    public async Task<Result> DeleteBucketAsync(
        string bucketName,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bucketName);

        try
        {
            await s3.DeleteBucketAsync(bucketName, cancellationToken);

            LogBucketDeleted(logger, bucketName);

            return Result.Success();
        }
        catch (AmazonS3Exception ex)
        {
            LogBucketDeletionFailed(logger, ex, bucketName, ex.ErrorCode);

            return ex.ErrorCode switch
            {
                "NoSuchBucket" => Result.NotFound($"Bucket '{bucketName}' does not exist."),

                "BucketNotEmpty" => Result.Conflict($"Bucket '{bucketName}' is not empty."),

                "AccessDenied" => Result.Forbidden(
                    "The storage provider denied permission to delete the bucket"
                ),

                _ => Result.Error("Failed to delete the storage bucket."),
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            UnexpectedErrorDeletingBucket(logger, bucketName, ex);

            return Result.Error("An unexpected error occurred while deleting the bucket.");
        }
    }

    #endregion

    #region Presigned URLs

    public async Task<Result<string>> GetPreSignedUploadUrlAsync(
        string objectKey,
        long expiresInSeconds,
        string? contentType = null,
        CancellationToken cancellationToken = default
    )
    {
        return await GetPreSignedUploadUrlAsync(
            DefaultBucketName,
            objectKey,
            expiresInSeconds,
            contentType,
            cancellationToken
        );
    }

    public async Task<Result<string>> GetPreSignedUploadUrlAsync(
        string bucket,
        string objectKey,
        long expiresInSeconds,
        string? contentType = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bucket);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectKey);

        var validationResult = ValidateExpiry(expiresInSeconds);

        if (!validationResult.IsSuccess)
        {
            return validationResult.Map();
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucket,
                Key = objectKey,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddSeconds(expiresInSeconds),
            };

            if (!string.IsNullOrWhiteSpace(contentType))
            {
                request.ContentType = contentType;
            }

            var url = await s3.GetPreSignedURLAsync(request);

            return Result<string>.Success(url);
        }
        catch (Exception ex)
        {
            LogPresignedUrlCreateFailed(logger, ex, objectKey, DefaultBucketName);

            return Result<string>.Error("Failed to generate the upload URL.");
        }
    }

    public async Task<Result<string>> GetPreSignedDownloadUrlAsync(
        string objectKey,
        long expiresInSeconds,
        CancellationToken cancellationToken = default
    )
    {
        return await GetPreSignedDownloadUrlAsync(
            DefaultBucketName,
            objectKey,
            expiresInSeconds,
            cancellationToken
        );
    }

    public async Task<Result<string>> GetPreSignedDownloadUrlAsync(
        string bucket,
        string objectKey,
        long expiresInSeconds,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectKey);

        var validationResult = ValidateExpiry(expiresInSeconds);

        if (!validationResult.IsSuccess)
        {
            return validationResult.Map();
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = DefaultBucketName,
                Key = objectKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddSeconds(expiresInSeconds),
            };

            var url = await s3.GetPreSignedURLAsync(request);

            return Result<string>.Success(url);
        }
        catch (Exception ex)
        {
            LogPresignedUrlCreateFailed(logger, ex, objectKey, DefaultBucketName);

            return Result<string>.Error("Failed to generate the download URL.");
        }
    }

    // Compatibility methods for existing consumers.

    public async Task<string> GetDownloadUrlAsync(
        string bucketName,
        string objectKey,
        int expiryMinutes = 60
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bucketName);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectKey);

        ValidateExpiryMinutes(expiryMinutes);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
        };

        return await s3.GetPreSignedURLAsync(request);
    }

    public async Task<string> GetDownloadUrlAsync(string objectKey, int expiryMinutes = 60)
    {
        return await GetDownloadUrlAsync(DefaultBucketName, objectKey, expiryMinutes);
    }

    #endregion

    #region Objects

    public Task DeleteFileAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken = default
    )
    {
        return DeleteObjectOrThrowAsync(bucketName, objectKey, cancellationToken);
    }

    public Task DeleteFileAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        return DeleteFileAsync(DefaultBucketName, objectKey, cancellationToken);
    }

    public async Task<Result> DeleteObjectAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bucketName);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectKey);

        try
        {
            var request = new DeleteObjectRequest { BucketName = bucketName, Key = objectKey };

            await s3.DeleteObjectAsync(request, cancellationToken);

            LogFileDeleted(logger, objectKey, bucketName);

            return Result.Success();
        }
        catch (AmazonS3Exception ex)
        {
            LogFileDeleteFailed(logger, ex, objectKey, bucketName, ex.ErrorCode);

            return ex.ErrorCode switch
            {
                "NoSuchBucket" => Result.NotFound($"Bucket '{bucketName}' does not exist."),

                "AccessDenied" => Result.Forbidden(
                    "The storage provider denied permission to delete the object."
                ),

                _ => Result.Error("Failed to delete the storage object."),
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogUnexpectedErrorDeletingObject(logger, ex, objectKey, bucketName);

            return Result.Error("An unexpected error occurred while deleting the object.");
        }
    }

    public async Task<Stream> GetObjectAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var result = await s3.GetObjectStreamAsync(
                bucketName,
                objectKey,
                new Dictionary<string, object>(),
                cancellationToken: cancellationToken
            );
            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AmazonS3Exception ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                LogObjectNotFound(logger, bucketName, objectKey);
            }

            LogObjectRetrievalFailed(logger, ex, bucketName, objectKey);

            throw;
        }
        catch (Exception ex)
        {
            LogObjectRetrievalUnexpectedFailure(logger, ex, bucketName, objectKey);

            throw;
        }
    }

    public async Task<Result<FileObjectMetadata>> GetObjectMetadataAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var response = await s3.GetObjectMetadataAsync(
                bucketName,
                objectKey,
                cancellationToken
            );

            var customMetadata = response
                .Metadata.Keys.Where(key =>
                    key.StartsWith(MetadataPrefix, StringComparison.OrdinalIgnoreCase)
                )
                .ToDictionary(
                    key => key[MetadataPrefix.Length..],
                    key => response.Metadata[key],
                    StringComparer.OrdinalIgnoreCase
                );

            return new FileObjectMetadata
            {
                SizeInBytes = response.ContentLength,
                ContentType = response.ContentType ?? string.Empty,
                ETag = response.ETag ?? string.Empty,
                LastModified = response.LastModified ?? DateTimeOffset.MinValue,
                Metadata = customMetadata,
                VersionId = response.VersionId,
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AmazonS3Exception ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                LogObjectNotFound(logger, bucketName, objectKey);

                return Result<FileObjectMetadata>.NotFound("The requested file was not found");
            }

            LogS3MetadataRetrievalFailed(logger, ex, bucketName, objectKey);

            return Result<FileObjectMetadata>.Error("Failed to retrieve file metadata.");
        }
        catch (Exception ex)
        {
            LogMetadataRetrievalFailed(logger, ex, bucketName, objectKey);

            return Result<FileObjectMetadata>.Error(
                "An unexpected error occurred while retrieving file metadata."
            );
        }
    }

    private async Task DeleteObjectOrThrowAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken
    )
    {
        var result = await DeleteObjectAsync(bucketName, objectKey, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Failed to delete object '{objectKey}' from bucket '{bucketName}'."
            );
        }
    }

    #endregion

    #region Validation

    private static Result ValidateExpiry(long expiresInSeconds)
    {
        if (expiresInSeconds is >= 1 and <= MaxExpirySeconds)
        {
            return Result.Success();
        }

        return Result.Invalid(
            new ValidationError(
                nameof(expiresInSeconds),
                "Expiration must be between 1 second and 604800 seconds (7 days)."
            )
        );
    }

    private static void ValidateExpiryMinutes(int expiryMinutes)
    {
        if (expiryMinutes is < 1 or > MaxExpiryMinutes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expiryMinutes),
                "Expiry must be between 1 minute and 7 days."
            );
        }
    }

    #endregion
}
