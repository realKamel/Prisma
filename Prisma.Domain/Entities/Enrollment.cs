using System;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class Enrollment : BaseEntity
{
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public string? EnrollmentMethod { get; set; }
    
    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    public DateTime? ExDate { get; set; } 

}