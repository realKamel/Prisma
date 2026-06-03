using System.Collections.Generic;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class Section : BaseEntity
{
    public string? LessonId { get; set; }
    public string? Title { get; set; } 
    public string? ContentURL { get; set; }
    public bool IsCompleted { get; set; }

    public virtual Lesson? Lesson { get; set; }
    public virtual ICollection<LessonProgress> Progresses { get; set; } = new List<LessonProgress>();
}