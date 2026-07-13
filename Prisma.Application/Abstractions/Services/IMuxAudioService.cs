namespace Prisma.Application.Abstractions.Services;

public interface IMuxAudioService
{
    Task StreamAudioToDestinationAsync(string playbackId, string token);
}