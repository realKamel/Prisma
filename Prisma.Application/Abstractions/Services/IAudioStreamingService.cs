namespace Prisma.Application.Abstractions.Services;

public interface IAudioStreamingService
{
    Task<Stream> StreamAudioAsync(string playbackId);
}
