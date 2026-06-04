using System;
using System.Collections.Generic;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class Assignment : BaseEntity
{
    public string? LessonId { get; set; }
    public virtual Lesson? Lesson { get; set; }
    
    public string? ContentURL { get; set; }
    public DateTime DueDate { get; set; }

    public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
}