using System;
using System.Collections.Generic;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class Assignment : BaseEntity
{
    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    
    public string? ContentURL { get; set; }
    public DateTime DueDate { get; set; }

    public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
}