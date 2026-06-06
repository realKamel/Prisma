using System.Collections.Generic;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class Section : BaseEntity
{
    public string Title { get; set; }
    public string? ContentURL { get; set; }

    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    public ICollection<SectionProgress> Progresses { get; set; } = new List<SectionProgress>();
}