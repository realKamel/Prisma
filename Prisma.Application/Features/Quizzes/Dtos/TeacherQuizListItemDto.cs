namespace Prisma.Application.Features.Quizzes.Dtos;

public class TeacherQuizListItemDto
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public int QuestionsCount { get; set; }
    public decimal TotalDegree { get; set; }
    public DateTimeOffset? AvailableFrom { get; set; }
    public DateTimeOffset? DueDate { get; set; }

    // Attempts summary
    public int SubmittedCount { get; set; }      // Submitted + Graded (total who attempted)
    public int PendingGradingCount { get; set; } // Status == Submitted (needs manual grading)
    public double? AverageScore { get; set; }    // null if no Graded attempts yet

    // Computed status
    public string Status { get; set; } = string.Empty; // "active" | "pending_grading" | "completed"
}


public class TeacherQuizzesListResponseDto
{
    public List<TeacherQuizListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}