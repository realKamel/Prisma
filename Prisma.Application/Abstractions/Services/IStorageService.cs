namespace Prisma.Application.Abstractions.Services;

public interface IStorageService
{
    Task UploadFileAsync(string bucketName, string objectKey, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task<string> GetDownloadUrlAsync(string bucketName, string objectKey, int expiryMinutes = 60);
    string GetPublicUrl(string bucketName, string objectKey);
    Task SetPublicReadPolicyAsync(string bucketName, params string[] publicPrefixes);
    Task DeleteFileAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default);
}