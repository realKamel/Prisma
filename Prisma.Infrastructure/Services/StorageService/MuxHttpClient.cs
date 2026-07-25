using Prisma.Application.Abstractions.Services;

namespace Prisma.Infrastructure.Services.StorageService;

public sealed class MuxHttpClient(HttpClient httpClient, IVideoStorageService videoStorageService)
    : IAudioStreamingService
{
    public async Task<Stream> StreamAudioAsync(string playbackId)
    {
        //var relativeDownloadPath = $"/{playbackId}/audio.m4a?token={token}";
        var relativeDownloadPath = await videoStorageService.GetAudioUrlAsync(playbackId);

        var downloadResponse = await httpClient.GetAsync(
            relativeDownloadPath,
            HttpCompletionOption.ResponseHeadersRead
        );
        try
        {
            downloadResponse.EnsureSuccessStatusCode();

            // Return the live, un-buffered network stream
            return await downloadResponse.Content.ReadAsStreamAsync();
        }
        catch
        {
            // If something failed before we could hand off the stream, 
            // clean up the response manually so we don't leak memory.
            downloadResponse.Dispose();
            throw;
        }
    }
}