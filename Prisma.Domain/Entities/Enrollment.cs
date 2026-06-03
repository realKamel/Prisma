using System;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class Enrollment : BaseEntity
{
    public string? StudentId { get; set; }
    public virtual Student? Student { get; set; }

    public string? EnrollmentMethod { get; set; }
    
    public string? LessonId { get; set; }
    public virtual Lesson? Lesson { get; set; }

    public DateTime? ExDate { get; set; } 

}