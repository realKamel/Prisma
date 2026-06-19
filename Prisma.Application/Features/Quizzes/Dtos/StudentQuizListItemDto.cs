namespace Prisma.Application.Features.Quizzes.Dtos;

public class StudentQuizListItemDto
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "new" | "pending" | "done" | "missed"
    public DateTimeOffset? ScheduledDate { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public decimal? Score { get; set; }
    public decimal TotalDegree { get; set; }
    public int QuestionsCount { get; set; }
    public int DurationMinutes { get; set; }
}
