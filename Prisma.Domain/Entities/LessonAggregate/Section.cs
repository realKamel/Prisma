using Prisma.Domain.Common;

namespace Prisma.Domain.Entities.LessonAggregate;

public class Section : BaseEntity
{
    public string Title { get; set; }
    public string? ContentURL { get; set; }

    public int LessonId { get; set; }
    public Lesson Lesson { get; set; }

    public ICollection<SectionProgress> Progresses { get; set; } = new List<SectionProgress>();
}