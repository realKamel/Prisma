using System;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class Code : BaseEntity
{
    public string? CodeValue { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    
    public string? StudentId { get; set; }    
    public virtual Student? Student { get; set; } 

    public string? LessonId { get; set; } 
    public virtual Lesson? Lesson { get; set; }
    
}