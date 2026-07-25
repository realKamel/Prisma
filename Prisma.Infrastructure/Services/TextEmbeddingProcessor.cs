using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel.Text;
using Prisma.Application.Abstractions.Ai;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Infrastructure.Services;

public class TextEmbeddingProcessor(IEmbeddingService embeddingService) : ITextEmbeddingProcessor
{
    [Experimental("SKEXP0050")]
    public async Task<List<LessonTranscriptChunk>> ProcessTextAsync(int lessonId, string rawScript,
        CancellationToken ct = default)
    {
        var lines = TextChunker
            .SplitPlainTextLines(rawScript, maxTokensPerLine: 100);

        var chunks = TextChunker
            .SplitPlainTextParagraphs(lines, maxTokensPerParagraph: 800, overlapTokens: 100);

        if (chunks.Count == 0) return [];

        // 2. Batch generate embeddings using your EmbeddingService
        var embeddings = await embeddingService.EmbedBatchAsync(chunks, ct);

        // 3. Map to your LessonTranscriptChunk entity
        var chunkEntities = new List<LessonTranscriptChunk>(chunks.Count);
        chunkEntities.AddRange(
            chunks.Select((t, i) =>
                LessonTranscriptChunk
                    .Create(lessonId: lessonId,
                        content: t,
                        index: i,
                        embedding: embeddings[i].ToArray())));

        return chunkEntities;
    }
}