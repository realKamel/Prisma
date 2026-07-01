using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Infrastructure.Services.StorageService;

public class MinioVideoStorageService(IAmazonS3 s3, IConfiguration configuration) : IVideoStorageService
{
    private readonly string _bucketName = configuration["VideoStorage:BucketName"]!;

    public async Task<string> UploadVideoAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false
        };

        await s3.PutObjectAsync(request, cancellationToken);
        return objectKey;
    }

    public async Task<string> GetVideoUrlAsync(string objectKey)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Expires = DateTime.UtcNow.AddHours(2),
            Verb = HttpVerb.GET
        };

        return await s3.GetPreSignedURLAsync(request);
    }

    public async Task DeleteVideoAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey
        };

        await s3.DeleteObjectAsync(request, cancellationToken);
    }
}