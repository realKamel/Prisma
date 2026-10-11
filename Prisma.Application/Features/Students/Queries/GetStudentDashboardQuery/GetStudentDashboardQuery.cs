using Ardalis.Result;
using MediatR;
using Prisma.Application.Common.DTOs;

namespace Prisma.Application.Features.Students.Queries.GetStudentDashboardQuery;

public record GetStudentDashboardQuery : IRequest<Result<GetStudentDashboardResponse>>;

public record GetStudentDashboardResponse
{
    public StudentDto Student { get; init; } = null!;
    public StreakDto Streak { get; init; } = null!;
    public NextLessonDto? NextLesson { get; init; }
    public List<LessonCardDto> Lessons { get; init; } = [];
    public StatsDto Stats { get; init; } = null!;
}

public record StudentDto
{
    public string FirstName { get; init; } = string.Empty;
    public string GradeLabel { get; init; } = string.Empty;
}

public record StreakDto
{
    public int Count { get; init; }
}

public record NextLessonDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string TeacherName { get; init; } = string.Empty;
    public string TeacherInitial { get; init; } = string.Empty;
    public int CurrentChapter { get; init; }
    public int TotalChapters { get; init; }
    public string PosterUrl { get; init; } = string.Empty;
}

public record LessonCardDto
{
    public int Id { get; init; }
    public Guid PublicId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string TeacherName { get; init; } = string.Empty;
    public string TeacherInitial { get; init; } = string.Empty;
    public TimeDurationDto? Duration { get; init; }
    public string Status { get; init; }
    public string PosterUrl { get; init; } = string.Empty;
    public int? ExpiresInDays { get; init; }
}

public record StatsDto
{
    public int PurchasedLessons { get; init; }
    public int CompletedLessons { get; init; }
    public int StudyHours { get; init; }
    public int TopQuizScore { get; init; }
}

public enum LessonStatus
{
    New,
    Progress,
    Done,
    Warn,
    Expired,
}
