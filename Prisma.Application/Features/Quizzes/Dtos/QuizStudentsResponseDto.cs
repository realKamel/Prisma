namespace Prisma.Application.Features.Quizzes.Dtos;

public class QuizStudentsResponseDto
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal TotalDegree { get; set; }
    public int TotalStudents { get; set; }
    public int SubmittedCount { get; set; }
    public int PendingGradingCount { get; set; }
    public int GradedCount { get; set; }
    public List<QuizStudentAttemptDto> Students { get; set; } = new();
    public int TotalCount { get; set; }   // for pagination
    public int Page { get; set; }
    public int PageSize { get; set; }
}
