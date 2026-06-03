using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class LessonProgress : BaseEntity
{
    public string? StudentId { get; set; }
    public virtual Student? Student { get; set; }

    public string? LessonId { get; set; }
    public virtual Lesson? Lesson { get; set; }

    public string? SectionId { get; set; }
    public virtual Section? Section { get; set; }
}