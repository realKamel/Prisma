using Prisma.Domain.Common;

namespace Prisma.Domain.Entities.LessonAggregate;

public sealed class LessonTranscriptChunk : IEntity<Guid>, IAuditable
{
    public Guid Id { get; set; }
    public int LessonId { get; private set; }
    public Lesson Lesson { get; private set; } = null!;
    public string Content { get; private set; } = string.Empty;
    public int ChunkIndex { get; private set; }
    public float[] Embedding { get; private set; }

    private LessonTranscriptChunk() { }

    public static LessonTranscriptChunk Create(
        int lessonId,
        string content,
        int index,
        float[] embedding
    )
    {
        return new LessonTranscriptChunk
        {
            Id = Guid.CreateVersion7(),
            LessonId = lessonId,
            Content = content,
            ChunkIndex = index,
            Embedding = embedding,
        };
    }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public Guid? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
}
