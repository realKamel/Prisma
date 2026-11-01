using Microsoft.Extensions.Logging;

namespace Prisma.Infrastructure.Services.ObjectStorageServices;

internal sealed partial class R2StorageService
{
    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Information,
        Message = "Successfully created bucket {BucketName}"
    )]
    private static partial void LogBucketCreated(ILogger logger, string bucketName);

    [LoggerMessage(
        EventId = 4002,
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
        EventId = 4003,
        Level = LogLevel.Information,
        Message = "Successfully deleted bucket {BucketName}"
    )]
    private static partial void LogBucketDeleted(ILogger logger, string bucketName);

    [LoggerMessage(
        EventId = 4004,
        Level = LogLevel.Error,
        Message = "Failed to generate presigned URL for object {ObjectKey} in bucket {BucketName}"
    )]
    private static partial void LogPresignedUrlCreateFailed(
        ILogger logger,
        Exception exception,
        string objectKey,
        string bucketName
    );

    [LoggerMessage(
        EventId = 4005,
        Level = LogLevel.Information,
        Message = "Successfully deleted object {ObjectKey} from bucket {BucketName}"
    )]
    private static partial void LogFileDeleted(ILogger logger, string objectKey, string bucketName);

    [LoggerMessage(
        EventId = 4006,
        Level = LogLevel.Error,
        Message = "Failed to delete object {ObjectKey} from bucket {BucketName}. AWS error code: {ErrorCode}"
    )]
    private static partial void LogFileDeleteFailed(
        ILogger logger,
        Exception exception,
        string objectKey,
        string bucketName,
        string? errorCode
    );

    [LoggerMessage(
        EventId = 4007,
        Level = LogLevel.Error,
        Message = "Unexpected error deleting object {ObjectKey} from bucket {BucketName}"
    )]
    private static partial void LogUnexpectedErrorDeletingObject(
        ILogger logger,
        Exception exception,
        string objectKey,
        string bucketName
    );

    [LoggerMessage(
        EventId = 4008,
        Level = LogLevel.Error,
        Message = "Unexpected error creating bucket {BucketName}"
    )]
    private static partial void UnexpectedErrorCreatingBucket(
        ILogger logger,
        string bucketName,
        Exception exception
    );

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
        EventId = 4010,
        Level = LogLevel.Error,
        Message = "Unexpected error deleting bucket {BucketName}"
    )]
    private static partial void UnexpectedErrorDeletingBucket(
        ILogger logger,
        string bucketName,
        Exception exception
    );

    [LoggerMessage(
        EventId = 4012,
        Level = LogLevel.Error,
        Message = "Failed to upload object '{ObjectKey}' to bucket '{BucketName}'. Error code: {ErrorCode}"
    )]
    private partial void LogUploadFailed(
        string bucketName,
        string objectKey,
        string? errorCode,
        Exception exception
    );

    [LoggerMessage(
        EventId = 4011,
        Level = LogLevel.Error,
        Message = "Unexpected error uploading object '{ObjectKey}' to bucket '{BucketName}'"
    )]
    private partial void LogUnexpectedUploadFailure(
        string bucketName,
        string objectKey,
        Exception exception
    );
}
