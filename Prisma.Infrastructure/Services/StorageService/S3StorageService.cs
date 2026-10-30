using Amazon.S3;
using Amazon.S3.Model;
using Ardalis.Result;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Infrastructure.Services.StorageService;

internal sealed partial class S3StorageService(
    IAmazonS3 s3,
    IOptions<ObjectStorageOptions> objectStorageOptions,
    ILogger<S3StorageService> logger,
    IHostEnvironment environment
) : IStorageService
{
    public string DefaultBucketName => objectStorageOptions.Value.BucketName;

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
        var url = await s3.GetPreSignedURLAsync(request);
        // var storageConfig = _config.GetSection("ObjectStorage");
        // var serviceUrl = objectStorageOptions.Value.ServiceUrl;

        if (url.StartsWith(@"https://", StringComparison.InvariantCultureIgnoreCase))
        {
            url = url.Replace(@"https://", @"http://", StringComparison.InvariantCultureIgnoreCase);
        }

        return url;
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

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Successfully deleted object '{ObjectKey}' from bucket '{BucketName}'"
    )]
    private static partial void LogFileDeleted(ILogger logger, string objectKey, string bucketName);

    [LoggerMessage(
        EventId = 2002,
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
        EventId = 2003,
        Level = LogLevel.Error,
        Message = "Failed to create presigned URL for object '{ObjectKey}' in bucket '{BucketName}'"
    )]
    private static partial void LogPresignedUrlCreateFailed(
        ILogger logger,
        string objectKey,
        string bucketName,
        Exception ex
    );
}
