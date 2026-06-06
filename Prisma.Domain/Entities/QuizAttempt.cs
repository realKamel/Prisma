using System;
using System.Collections.Generic;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class QuizAttempt : BaseEntity
{
    public int QuizId { get; set; }
    public LessonQuiz? Quiz { get; set; } 

    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public decimal Degree { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? Status { get; set; } 

    public ICollection<AttemptAnswer> Answers { get; set; } = new List<AttemptAnswer>();
}