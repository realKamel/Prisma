using Prisma.Domain.Common;
using Prisma.Domain.DomainEvents.StreakEvents;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities.QuizAggregate;

public class QuizAttempt : BaseEntity
{
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; }

    public Guid StudentId { get; set; }
    public Student Student { get; set; }

    public decimal Degree { get; set; }
    public decimal PenaltyScore { get; set; } = 0;

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset Deadline { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }

    public QuizAttemptStatus Status { get; set; }

    public ICollection<AttemptAnswer> Answers { get; set; } = new List<AttemptAnswer>();

    public int TabSwitchCount { get; set; }
    public int CopyPasteAttemptCount { get; set; }
    public uint Version { get; set; }

    public static QuizAttempt Start(int quizId, Guid studentId, DateTimeOffset now, TimeSpan duration)
    {
        var attempt = new QuizAttempt
        {
            QuizId = quizId,
            StudentId = studentId,
            StartedAt = now,
            Deadline = now + duration,
            Status = QuizAttemptStatus.InProgress,
        };
        attempt.AddDomainEvent(new QuizAttemptStartedEvent(studentId));
        return attempt;
    }
}