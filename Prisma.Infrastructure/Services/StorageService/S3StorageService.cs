using Amazon.S3;
using Amazon.S3.Model;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Infrastructure.Services.StorageService;

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;

    public S3StorageService(IAmazonS3 s3)
    {
        _s3 = s3;
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

    public async Task<string> GetDownloadUrlAsync(string bucketName, string objectKey, int expiryMinutes = 60)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Verb = HttpVerb.GET
        };

        return await _s3.GetPreSignedURLAsync(request);
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