using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Infrastructure.Services.StorageService;

internal sealed class MinioVideoStorageService(
    IAmazonS3 s3,
    IOptions<ObjectStorageOptions> objectStorage) : IVideoStorageService
{
    private readonly string _bucketName = objectStorage.Value.BucketName;

    private readonly string _serviceUrl = objectStorage.Value.ServiceUrl;

    public async Task<VideoUploadResult> GetUploadUrlAsync(int sectionId, string? guidId,
        CancellationToken cancellationToken = default)
    {
        // var guid = Guid.NewGuid().ToString("N");
        var guid = guidId?.Split('.')[0] ?? Guid.NewGuid().ToString("N");

        var objectKey = $"uploads/{guid}.mp4";

        // Generate a presigned PUT URL (valid for 15 minutes)
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            // ContentType = "video/mp4",
            Expires = DateTime.UtcNow.AddMinutes(15),
            // Metadata = { ["section-id"] = sectionId.ToString() }
        };

        var uploadUrl = await s3.GetPreSignedURLAsync(request);
        uploadUrl = uploadUrl.Replace("https", "http", StringComparison.InvariantCultureIgnoreCase);
        // Return the GUID as the ID. The frontend will PUT the file to 'uploadUrl'.
        return new VideoUploadResult(uploadUrl, guid);
    }

    public Task<string> GetVideoUrlAsync(string playbackId)
    {
        // playbackId is the GUID. Matches your "videos" PublicPrefix.
        // Because ForcePathStyle is true, the URL format is: {ServiceUrl}/{BucketName}/{Key}
        var url = $"{_serviceUrl}/{_bucketName}/videos/{playbackId}/index.m3u8";
        return Task.FromResult(url);
    }

    public Task<string> GetAudioUrlAsync(string playbackId)
    {
        var url = $"{_serviceUrl}/{_bucketName}/videos/{playbackId}/audio.m4a";
        return Task.FromResult(url);
    }

    public async Task DeleteVideoAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        // 1. Delete the raw upload (if the worker hasn't already)
        var uploadKey = $"uploads/{objectKey}.mp4";
        try { await s3.DeleteObjectAsync(_bucketName, uploadKey, cancellationToken); }
        catch
        {
            /* Ignore if already deleted by worker */
        }

        // 2. Delete all HLS segments and playlists in the videos/{guid}/ folder
        var listResponse = await s3.ListObjectsV2Async(
            new ListObjectsV2Request { BucketName = _bucketName, Prefix = $"videos/{objectKey}/" }, cancellationToken);

        if (listResponse.S3Objects.Count > 0)
        {
            var deleteRequest = new DeleteObjectsRequest
            {
                BucketName = _bucketName,
                Objects = [.. listResponse.S3Objects.Select(o => new KeyVersion { Key = o.Key })]
            };
            await s3.DeleteObjectsAsync(deleteRequest, cancellationToken);
        }
    }
}