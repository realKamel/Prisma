namespace Prisma.Application.Features.Assignments.Dtos;

public class AssignmentSubmissionListItemDto
{
    public int SubmissionId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int AssignmentId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public int? Score { get; set; }
    public int MaxScore { get; set; }
    public string Status { get; set; } = string.Empty; // "not_submitted" | "pending" | "grading" | "graded"

    // Grading lock info
    public bool IsBeingGraded { get; set; }
    public string? GradingByUserName { get; set; }
}
