using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Ardalis.Result;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prisma.Application.Abstractions.ObjectStorage;

namespace Prisma.Infrastructure.Services.StorageService;

public sealed partial class S3StorageService(
    IAmazonS3 s3,
    IOptions<ObjectStorageOptions> objectStorageOptions,
    ILogger<S3StorageService> logger,
    IHostEnvironment environment
) : IStorageService
{
    public string DefaultBucketName => objectStorageOptions.Value.BucketName;

    public async Task<Result> CreateBucketAsync(
        string bucketName,
        CancellationToken cancellationToken = default
    )
    {
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

            return Result.Error("An unexpected error occurred while creating the bucket");
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
                "NoSuchBucket" => Result.NotFound($"Bucket '{bucketName}' does not exist"),

                "BucketNotEmpty" => Result.Conflict($"Bucket '{bucketName}' is not empty"),

                "AccessDenied" => Result.Forbidden(
                    "The storage provider denied permission to delete the bucket"
                ),

                _ => Result.Error("Failed to delete the storage bucket"),
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            UnexpectedErrorDeletingBucket(logger, bucketName, ex);

            return Result.Error("An unexpected error occurred while deleting the bucket");
        }
    }

    public async Task UploadFileAsync(
        string bucketName,
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default
    )
    {
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false,
        };

        await s3.PutObjectAsync(request, cancellationToken);
    }

    public async Task UploadFileAsync(
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default
    )
    {
        await UploadFileAsync(
            DefaultBucketName,
            objectKey,
            content,
            contentType,
            cancellationToken
        );
    }

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
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = DefaultBucketName,
                Key = objectKey,
                Expires = DateTime.UtcNow.AddSeconds(expiresInSeconds),
                Verb = HttpVerb.PUT,
            };

            if (environment.IsDevelopment())
            {
                request.Protocol = Protocol.HTTP;
            }

            var link = await s3.GetPreSignedURLAsync(request);
            return link;
        }
        catch (Exception ex)
        {
            LogPresignedUrlCreateFailed(logger, objectKey, DefaultBucketName, ex);
            return Result.Error("Error happened while create upload link");
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
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = DefaultBucketName,
                Key = objectKey,
                Expires = DateTime.UtcNow.AddSeconds(expiresInSeconds),
                Verb = HttpVerb.GET,
            };

            if (environment.IsDevelopment())
            {
                request.Protocol = Protocol.HTTP;
            }

            var link = await s3.GetPreSignedURLAsync(request);
            return link;
        }
        catch (Exception ex)
        {
            LogPresignedUrlCreateFailed(logger, objectKey, DefaultBucketName, ex);
            return Result.Error("Error happened while create download link");
        }
    }

    // public string GetPublicUrl(string bucketName, string objectKey)
    // {
    //     var storageConfig = _config.GetSection("ObjectStorage");
    //     return $"{storageConfig["ServiceUrl"]!.TrimEnd('/')}/{bucketName}/{objectKey}";
    // }
    public async Task<string> GetDownloadUrlAsync(
        string bucketName,
        string objectKey,
        int expiryMinutes = 60
    )
    {
        if (string.IsNullOrWhiteSpace(bucketName))
        {
            bucketName = DefaultBucketName;
        }

        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Verb = HttpVerb.GET,
        };
        request.Protocol = environment.IsDevelopment() ? Protocol.HTTP : Protocol.HTTPS;
        return await s3.GetPreSignedURLAsync(request);

        // if (url.StartsWith(@"https://", StringComparison.InvariantCultureIgnoreCase))
        // {
        //     url = url.Replace(@"https://", @"http://", StringComparison.InvariantCultureIgnoreCase);
        // }
    }

    public async Task<string> GetDownloadUrlAsync(string objectKey, int expiryMinutes = 60)
    {
        return await GetDownloadUrlAsync(DefaultBucketName, objectKey, expiryMinutes);
    }

    // public async Task SetPublicReadPolicyAsync(string bucketName, params string[] publicPrefixes)
    // {
    //     var statements = publicPrefixes.Select(prefix => new
    //     {
    //         Effect = "Allow",
    //         Principal = new { AWS = new[] { "*" } },
    //         Action = new[] { "s3:GetObject" },
    //         Resource = new[] { $"arn:aws:s3:::{bucketName}/{prefix.TrimEnd('/')}/*" }
    //     });

    //     var policy = new { Version = "2012-10-17", Statement = statements };
    //     var policyJson = System.Text.Json.JsonSerializer.Serialize(policy);

    //     await _s3.PutBucketPolicyAsync(bucketName, policyJson);
    // }
    public async Task DeleteFileAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var request = new DeleteObjectRequest { BucketName = bucketName, Key = objectKey };
            await s3.DeleteObjectAsync(request, cancellationToken);
            LogFileDeleted(logger, objectKey, bucketName);
        }
        catch (Exception e)
        {
            LogFileDeleteFailed(logger, objectKey, bucketName, e);
            throw;
        }
    }

    public async Task DeleteFileAsync(
        string objectKey,
        CancellationToken cancellationToken = default
    )
    {
        await DeleteFileAsync(DefaultBucketName, objectKey, cancellationToken);
    }

    public async Task<Result> DeleteObjectAsync(
        string objectKeys,
        string bucketName,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await DeleteFileAsync(
                bucketName: bucketName,
                objectKey: objectKeys,
                cancellationToken: cancellationToken
            );

            return Result.Success();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Result.Error("Error Deleting Object");
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
        var response = await s3.GetObjectMetadataAsync(bucketName, objectKey, cancellationToken);
        var customMetadata = response.Metadata.Keys.ToDictionary(
            key => key["x-amz-meta-".Length..],
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

    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Information,
        Message = "Successfully deleted object '{ObjectKey}' from bucket '{BucketName}'"
    )]
    private static partial void LogFileDeleted(ILogger logger, string objectKey, string bucketName);

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Error,
        Message = "Failed to delete object '{ObjectKey}' from bucket '{BucketName}'"
    )]
    private static partial void LogFileDeleteFailed(
        ILogger logger,
        string objectKey,
        string bucketName,
        Exception ex
    );

    [LoggerMessage(
        EventId = 4003,
        Level = LogLevel.Error,
        Message = "Failed to create presigned URL for object '{ObjectKey}' in bucket '{BucketName}'"
    )]
    private static partial void LogPresignedUrlCreateFailed(
        ILogger logger,
        string objectKey,
        string bucketName,
        Exception ex
    );

    [LoggerMessage(
        EventId = 4004,
        Level = LogLevel.Information,
        Message = "Successfully created bucket {BucketName}"
    )]
    private static partial void LogBucketCreated(ILogger logger, string bucketName);

    [LoggerMessage(
        EventId = 4005,
        Level = LogLevel.Error,
        Message = "Failed to create bucket {BucketName}. AWS error code: {ErrorCode}"
    )]
    private static partial void LogBucketCreationFailed(
        ILogger logger,
        Exception exception,
        string bucketName,
        string? errorCode
    );

    [LoggerMessage(
        EventId = 4006,
        Level = LogLevel.Information,
        Message = "Successfully deleted bucket {BucketName}"
    )]
    private static partial void LogBucketDeleted(ILogger logger, string bucketName);

    [LoggerMessage(
        EventId = 4009,
        Level = LogLevel.Error,
        Message = "Failed to delete bucket {BucketName}. AWS error code: {ErrorCode}"
    )]
    private static partial void LogBucketDeletionFailed(
        ILogger logger,
        Exception exception,
        string bucketName,
        string? errorCode
    );

    [LoggerMessage(
        EventId = 4019,
        Level = LogLevel.Error,
        Message = "Unexpected error deleting bucket {BucketName}"
    )]
    private static partial void UnexpectedErrorDeletingBucket(
        ILogger logger,
        string bucketName,
        Exception exception
    );

    [LoggerMessage(
        EventId = 4020,
        Level = LogLevel.Error,
        Message = "Unexpected error creating bucket {BucketName}"
    )]
    private static partial void UnexpectedErrorCreatingBucket(
        ILogger logger,
        string bucketName,
        Exception exception
    );

    [LoggerMessage(
        EventId = 4015,
        Level = LogLevel.Warning,
        Message = "S3 object not found. Bucket: {BucketName}, ObjectKey: {ObjectKey}"
    )]
    private static partial void LogObjectNotFound(
        ILogger logger,
        string bucketName,
        string objectKey
    );

    [LoggerMessage(
        EventId = 4013,
        Level = LogLevel.Error,
        Message = "S3 metadata retrieval failed. Bucket: {BucketName}, ObjectKey: {ObjectKey}"
    )]
    private static partial void LogS3MetadataRetrievalFailed(
        ILogger logger,
        Exception exception,
        string bucketName,
        string objectKey
    );

    [LoggerMessage(
        EventId = 4014,
        Level = LogLevel.Error,
        Message = "Unexpected error retrieving file metadata. Bucket: {BucketName}, ObjectKey: {ObjectKey}"
    )]
    private static partial void LogMetadataRetrievalFailed(
        ILogger logger,
        Exception exception,
        string bucketName,
        string objectKey
    );

    [LoggerMessage(
        EventId = 4016,
        Level = LogLevel.Error,
        Message = "S3 object retrieval failed. Bucket: {BucketName}, ObjectKey: {ObjectKey}"
    )]
    private static partial void LogObjectRetrievalFailed(
        ILogger logger,
        Exception exception,
        string bucketName,
        string objectKey
    );

    [LoggerMessage(
        EventId = 4017,
        Level = LogLevel.Error,
        Message = "Unexpected error retrieving object. Bucket: {BucketName}, ObjectKey: {ObjectKey}"
    )]
    private static partial void LogObjectRetrievalUnexpectedFailure(
        ILogger logger,
        Exception exception,
        string bucketName,
        string objectKey
    );
}
