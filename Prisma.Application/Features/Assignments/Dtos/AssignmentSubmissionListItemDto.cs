namespace Prisma.Application.Features.Assignments.Dtos;

public sealed record AssignmentSubmissionListItemDto
{
    public int SubmissionId { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public int AssignmentId { get; init; }
    public string LessonTitle { get; init; } = string.Empty;
    public string? FileUrl { get; init; }
    public DateTimeOffset? SubmittedAt { get; init; }
    public int? Score { get; init; }
    public decimal MaxScore { get; init; }
    public string Status { get; init; } = string.Empty; // "not_submitted" | "pending" | "grading" | "graded"

    // Grading lock info
    public bool IsBeingGraded { get; init; }
    public string? GradingByUserName { get; init; }
}
