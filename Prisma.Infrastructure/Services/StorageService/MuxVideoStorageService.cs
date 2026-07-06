using Microsoft.Extensions.Configuration;
using Mux.Csharp.Sdk.Api;
using Mux.Csharp.Sdk.Model;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Infrastructure.Services.StorageService;

public class MuxVideoStorageService(IConfiguration configuration, IMuxTokenService muxTokenService) : IVideoStorageService
{
    private readonly DirectUploadsApi _uploadsApi = new(new Mux.Csharp.Sdk.Client.Configuration
    {
        Username = configuration["Mux:TokenId"],
        Password = configuration["Mux:TokenSecret"]
    });

    private readonly AssetsApi _assetsApi = new(new Mux.Csharp.Sdk.Client.Configuration
    {
        Username = configuration["Mux:TokenId"],
        Password = configuration["Mux:TokenSecret"]
    });

    public async Task<VideoUploadResult> GetUploadUrlAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        var upload = await _uploadsApi.CreateDirectUploadAsync(new CreateUploadRequest
        {
            NewAssetSettings = new CreateAssetRequest
            {
                PlaybackPolicies = new List<PlaybackPolicy> { PlaybackPolicy.Signed },
                Passthrough = sectionId.ToString()
            },
            CorsOrigin = "*"
        });

        return new VideoUploadResult(upload.Data.Url, upload.Data.Id);
    }

    public Task<string> GetVideoUrlAsync(string playbackId)
    {
        var token = muxTokenService.GeneratePlaybackToken(playbackId);
        return Task.FromResult($"https://stream.mux.com/{playbackId}.m3u8?token={token}");
    }

    public async Task DeleteVideoAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        await _assetsApi.DeleteAssetAsync(objectKey);
    }
}