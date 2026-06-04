using System.Collections.Generic;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class Section : BaseEntity
{
    public string? Title { get; set; } 
    public string? ContentURL { get; set; }

    public required string LessonId { get; set; }
    public required virtual Lesson Lesson { get; set; }

    public virtual ICollection<SectionProgress> Progresses { get; set; } = new List<SectionProgress>();
}