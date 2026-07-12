using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Abstractions.Ai;
using Prisma.Application.Common.Constants;

namespace Prisma.Infrastructure.Ai;

public sealed class EmbeddingService(
    [FromKeyedServices(AIType.Embedding)] IEmbeddingGenerator<string, Embedding<float>> generator)
    : IEmbeddingService
{
    public async Task<ReadOnlyMemory<float>> EmbedAsync(string text, CancellationToken ct)
    {
        var result = await generator.GenerateAsync(text, cancellationToken: ct);
        return result.Vector;
    }

    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(
        IReadOnlyList<string> texts, CancellationToken ct)
    {
        var results = await generator.GenerateAsync(texts, cancellationToken: ct);
        return results.Select(r => r.Vector).ToList();
    }
}