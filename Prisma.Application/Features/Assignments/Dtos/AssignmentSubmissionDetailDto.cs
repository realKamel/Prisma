namespace Prisma.Application.Features.Assignments.Dtos;

public sealed record AssignmentSubmissionDetailDto
{
    // Display info
    public int SubmissionId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string LessonTitle { get; init; } = string.Empty;
    public DateTimeOffset SubmittedAt { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public bool IsLateSubmission { get; init; }
    public string? FileUrl { get; init; }
    public decimal MaxScore { get; init; }

    // Previous grading (null if not graded yet)
    public int? CurrentScore { get; init; }
    public string? CurrentNote { get; init; }
}
