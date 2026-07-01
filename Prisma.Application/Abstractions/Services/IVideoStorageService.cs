namespace Prisma.Application.Abstractions.Services;

public interface IVideoStorageService
{
    Task<string> UploadVideoAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task<string> GetVideoUrlAsync(string objectKey);
    Task DeleteVideoAsync(string objectKey, CancellationToken cancellationToken = default);
}