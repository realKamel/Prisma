using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Prisma.Domain.Entities;
public class Student : BaseEntity
{
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public string? ParentPhoneNumber { get; set; }

    public string? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    public string? AcademicYearId { get; set; }
    public AcademicYear? AcademicYear { get; set; }
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public virtual ICollection<SectionProgress> SectionProgresses { get; set; } = new List<SectionProgress>();
    public virtual ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Code> Codes { get; set; } = new List<Code>();
    public virtual ICollection<AttemptAnswer> AttemptAnswers { get; set; } = new List<AttemptAnswer>();
    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}

