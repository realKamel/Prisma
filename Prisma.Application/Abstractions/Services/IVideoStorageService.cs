namespace Prisma.Application.Abstractions.Services;

public interface IVideoStorageService
{
    Task<VideoUploadResult> GetUploadUrlAsync(int sectionId, CancellationToken cancellationToken = default);
    Task<string> GetVideoUrlAsync(string playbackId);
    Task DeleteVideoAsync(string assetId, CancellationToken cancellationToken = default);
}

public record VideoUploadResult(string UploadUrl, string UploadId);