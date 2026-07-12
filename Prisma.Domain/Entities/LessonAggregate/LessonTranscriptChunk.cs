using Prisma.Domain.Common;

namespace Prisma.Domain.Entities.LessonAggregate;

public class LessonTranscriptChunk : IEntity<Guid>
{
    public Guid Id { get; set; }
    public int LessonId { get; private set; }
    public Lesson Lesson { get; private set; } = null!;
    public string Content { get; private set; } = string.Empty;
    public int ChunkIndex { get; private set; }
    public float[] Embedding { get; private set; }

    private LessonTranscriptChunk()
    {
    }

    public static LessonTranscriptChunk Create(int lessonId, string content, int index, float[] embedding)
    {
        return new LessonTranscriptChunk
        {
            Id = Guid.CreateVersion7(),
            LessonId = lessonId,
            Content = content,
            ChunkIndex = index,
            Embedding = embedding
        };
    }
}