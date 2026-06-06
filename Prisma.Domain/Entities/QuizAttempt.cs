using Prisma.Domain.Common;

namespace Prisma.Domain.Entities;

public class QuizAttempt : BaseEntity
{
    public int QuizId { get; set; }
    public LessonQuiz? Quiz { get; set; }

    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public decimal Degree { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? SubmittedAt { get; set; }

    public string? Status { get; set; }

    public ICollection<AttemptAnswer> Answers { get; set; } = new List<AttemptAnswer>();
}