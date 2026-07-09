using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Abstractions.Ai;
using Prisma.Application.Common.Constants;

namespace Prisma.Infrastructure.Ai;

public sealed class TranscriptionService(
    [FromKeyedServices(AIType.SpeechToText)]
#pragma warning disable MEAI001
    ISpeechToTextClient client)
#pragma warning restore MEAI001
    : ITranscriptionService
{
    public async Task<string> TranscribeAsync(Stream audio, string fileName, CancellationToken ct)
    {
        var response = await client.GetTextAsync(audio, cancellationToken: ct);
        return response.Text;
    }
}