namespace Prisma.Application.Features.Quizzes.Dtos;

public class QuizStudentAttemptDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string AttemptStatus { get; set; } = string.Empty; // "not_started" | "in_progress" | "submitted" | "graded" | "missed"
    public DateTimeOffset? SubmittedAt { get; set; }
    public decimal? Score { get; set; }
    public decimal TotalDegree { get; set; }
    public int PendingWrittenCount { get; set; } // > 0 => show "صحح" button
}
