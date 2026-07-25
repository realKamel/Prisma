using Groq.Core.Clients;
using Groq.Core.Models;
using Prisma.Application.Abstractions.Ai;

namespace Prisma.Infrastructure.Ai;

public sealed class TranscriptionService(AudioClient audioClient)
    : ITranscriptionService
{
    public async Task<string> TranscribeAsync(Stream audio, string fileName, CancellationToken ct)
    {
        var result = await audioClient
            .CreateTranscriptionAsync(
                audioFile: audio,
                fileName: "",
                model: AudioModels.WHISPER_LARGE_V3_TURBO.Id,
                language: "ar",
                temperature: 0.0f
            );

        return result?["text"]?.ToString() ?? "";
    }
}