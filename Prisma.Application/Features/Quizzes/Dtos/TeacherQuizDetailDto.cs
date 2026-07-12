using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Dtos;

public class TeacherQuizDetailDto
{
    // Basic info
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Scope { get; set; } = string.Empty;
    public int? LessonId { get; set; }
    public string? LessonTitle { get; set; }
    public int? AcademicYearId { get; set; }
    public string? AcademicYearName { get; set; }
    public int DurationMinutes { get; set; }
    public decimal TotalDegree { get; set; }
    public DateTimeOffset? AvailableFrom { get; set; }
    public DateTimeOffset? DueDate { get; set; }

    // Attempts stats
    public int SubmittedCount { get; set; }
    public int PendingGradingCount { get; set; }
    public double? AverageScore { get; set; }
    public string Status { get; set; } = string.Empty;

    // Questions — only present in detail view
    public List<TeacherQuizQuestionDto> Questions { get; set; } = new();
}
public class TeacherQuizQuestionDto
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public decimal Degree { get; set; }
    public List<TeacherQuizChoiceDto>? Choices { get; set; }
    public string? ModelAnswer { get; set; }
}

public class TeacherQuizChoiceDto
{
    public int ChoiceId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}