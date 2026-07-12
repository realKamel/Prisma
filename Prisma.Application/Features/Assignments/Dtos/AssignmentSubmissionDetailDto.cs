namespace Prisma.Application.Features.Assignments.Dtos;

public class AssignmentSubmissionDetailDto
{
    // Display info
    public int SubmissionId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string LessonTitle { get; set; } = string.Empty;
    public DateTimeOffset SubmittedAt { get; set; }
    public DateTimeOffset DueDate { get; set; }
    public bool IsLateSubmission { get; set; }
    public string? FileUrl { get; set; }
    public int MaxScore { get; set; }

    // Previous grading (null if not graded yet)
    public int? CurrentScore { get; set; }
    public string? CurrentNote { get; set; }
}
