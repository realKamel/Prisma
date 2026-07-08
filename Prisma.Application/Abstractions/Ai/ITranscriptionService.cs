namespace Prisma.Application.Abstractions.Ai;

public interface ITranscriptionService
{
    Task<string> TranscribeAsync(Stream audio, string fileName, CancellationToken ct);
}