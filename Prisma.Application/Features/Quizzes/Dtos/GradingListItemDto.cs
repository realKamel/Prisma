namespace Prisma.Application.Features.Quizzes.Dtos;

public class GradingListItemDto
{
    public int AttemptId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public DateTimeOffset? SubmittedAt { get; set; }
    public string Status { get; set; } = string.Empty; // "submitted" | "graded"
    public decimal? Score { get; set; }
    public decimal TotalDegree { get; set; }
    public int PendingWrittenCount { get; set; }
}
