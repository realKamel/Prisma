using Prisma.Domain.Common;
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

    public DateTimeOffset? SubmittedAt { get; set; }

    public QuizAttemptStatus Status { get; set; }

    public ICollection<AttemptAnswer> Answers { get; set; } = new List<AttemptAnswer>();

    public int TabSwitchCount { get; set; }
    public int CopyPasteAttemptCount { get; set; }
}