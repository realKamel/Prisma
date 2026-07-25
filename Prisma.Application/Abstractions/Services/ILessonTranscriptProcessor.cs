using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Application.Abstractions.Services;

public interface ITextEmbeddingProcessor
{
    Task<List<LessonTranscriptChunk>> ProcessTextAsync(int lessonId, string rawScript,
        CancellationToken ct = default);
}