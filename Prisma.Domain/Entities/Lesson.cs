using System;
using System.Collections.Generic;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class Lesson : BaseEntity
{
    public string? Title { get; set; }

    public string? AcademicYearId { get; set; }
    public virtual AcademicYear? AcademicYear { get; set; }

    public string? Description { get; set; }
    public decimal Price { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsEligible { get; set; }

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
    public virtual ICollection<LessonProgress> Progresses { get; set; } = new List<LessonProgress>();
    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public virtual ICollection<LessonQuiz> Quizzes { get; set; } = new List<LessonQuiz>();
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    //public virtual ICollection<Code> Codes { get; set; } = new List<Code>();

}