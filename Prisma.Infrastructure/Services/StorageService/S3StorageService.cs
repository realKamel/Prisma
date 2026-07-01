using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Infrastructure.Services.StorageService;

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly IConfiguration _config;


    public S3StorageService(IAmazonS3 s3, IConfiguration configuration)
    {
        _s3 = s3;
        _config = configuration;
    }

    public async Task UploadFileAsync(string bucketName, string objectKey, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false
        };

        await _s3.PutObjectAsync(request, cancellationToken);
    }

    public string GetPublicUrl(string bucketName, string objectKey)
    {
        var storageConfig = _config.GetSection("Storage");
        return $"{storageConfig["ServiceUrl"]!.TrimEnd('/')}/{bucketName}/{objectKey}";
    }
    public async Task<string> GetDownloadUrlAsync(string bucketName, string objectKey, int expiryMinutes = 60)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Verb = HttpVerb.GET
        };
        var url = await _s3.GetPreSignedURLAsync(request);
        var storageConfig = _config.GetSection("Storage");

        if (storageConfig["ServiceUrl"]!.StartsWith("http://"))
            url = url.Replace("https://", "http://");
        return url;
    }
    public async Task SetPublicReadPolicyAsync(string bucketName, params string[] publicPrefixes)
    {
        var statements = publicPrefixes.Select(prefix => new
        {
            Effect = "Allow",
            Principal = new { AWS = new[] { "*" } },
            Action = new[] { "s3:GetObject" },
            Resource = new[] { $"arn:aws:s3:::{bucketName}/{prefix.TrimEnd('/')}/*" }
        });

        var policy = new { Version = "2012-10-17", Statement = statements };
        var policyJson = System.Text.Json.JsonSerializer.Serialize(policy);

        await _s3.PutBucketPolicyAsync(bucketName, policyJson);
    }
    public async Task DeleteFileAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey
        };

        await _s3.DeleteObjectAsync(request, cancellationToken);
    }
}