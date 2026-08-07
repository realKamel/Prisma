using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Dtos;

public class TeacherQuizDetailProjection
{
    public int QuizId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public QuizScope Scope { get; set; }

    public int? LessonId { get; set; }
    public string? LessonTitle { get; set; }

    public int? AcademicYearId { get; set; }
    public string? AcademicYearName { get; set; }

    public TimeSpan TimeInMinutes { get; set; }
    public decimal TotalDegree { get; set; }

    public DateTimeOffset? AvailableFrom { get; set; }
    public DateTimeOffset? DueDate { get; set; }

    public int PendingGradingCount { get; set; }
    public int SubmittedCount { get; set; }

    public decimal? AverageDegree { get; set; }

    public bool HasAttempts { get; set; }
    public bool HasUngradedAttempts { get; set; }

}


