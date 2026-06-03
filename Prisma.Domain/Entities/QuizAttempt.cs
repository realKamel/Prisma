using System;
using System.Collections.Generic;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class QuizAttempt : BaseEntity
{
    public string? QuizId { get; set; }
    public virtual LessonQuiz? Quiz { get; set; } 

    public string? StudentId { get; set; }
    public virtual Student? Student { get; set; }

    public decimal Degree { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? Status { get; set; } 

    public virtual ICollection<AttemptAnswer> Answers { get; set; } = new List<AttemptAnswer>();
}