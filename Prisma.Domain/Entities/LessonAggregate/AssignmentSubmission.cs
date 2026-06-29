using Prisma.Domain.Common;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Entities.LessonAggregate;

public class AssignmentSubmission : BaseEntity
{
    public Guid StudentId { get; set; }
    public Student Student { get; set; }

    public int AssignmentId { get; set; }
    public Assignment Assignment { get; set; }
    public int? Score {set; get;}
    public string? Notes {set; get;}  
    public string? FileUrl { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }

    // Grading lock — prevents two teachers grading the same submission simultaneously
    public bool IsBeingGraded { get; set; }
    public DateTimeOffset? GradingStartedAt { get; set; }
    public Guid? GradingByUserId { get; set; }
}