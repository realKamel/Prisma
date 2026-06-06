using Prisma.Domain.Common;

namespace Prisma.Domain.Entities;

public class Code : BaseEntity
{
    public string? CodeValue { get; set; }
    public bool IsUsed { get; set; }
    public DateTimeOffset? UsedAt { get; set; }

    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }
}