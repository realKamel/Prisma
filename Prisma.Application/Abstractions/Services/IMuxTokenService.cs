namespace Prisma.Application.Abstractions.Services;

public interface IMuxTokenService
{
    string GeneratePlaybackToken(string playbackId, int expiryHours = 6);
}