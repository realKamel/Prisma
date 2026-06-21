using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Students.Queries.GetStudentDashboardQuery;

public record GetStudentDashboardQuery : IRequest<Result<GetStudentDashboardResponse>>;


public class GetStudentDashboardResponse
{
    public StudentDto Student { get; init; } = default!;
    public StreakDto Streak { get; init; } = default!;
    public NextLessonDto? NextLesson { get; init; }
    public List<LessonCardDto> Lessons { get; init; } = [];
    public StatsDto Stats { get; init; } = default!;
}

public class StudentDto
{
    public string FirstName { get; init; } = default!;
    public string GradeLabel { get; init; } = default!;
}

public class StreakDto
{
    public int Count { get; init; }
}

public class NextLessonDto
{
    public string Id { get; init; } = default!;
    public string Title { get; init; } = default!;
    public int CurrentChapter { get; init; }
    public int TotalChapters { get; init; }
    public string PosterUrl { get; init; } = default!;
}

// ── Lesson Card ───────────────────────────────────────────────
public class LessonCardDto
{
    public string Id { get; init; } = default!;
    public string Title { get; init; } = default!;
    public string DurationLabel { get; init; } = default!;
    public LessonStatus Status { get; init; }
    public string PosterUrl { get; init; } = default!;
    public int? ExpiresInDays { get; init; }
}

// ── Stats ─────────────────────────────────────────────────────
public class StatsDto
{
    public int PurchasedLessons { get; init; }
    public int CompletedLessons { get; init; }
    public int StudyHours { get; init; }
    public int TopQuizScore { get; init; }
}

// ── Lesson Status Enum ────────────────────────────────────────
public enum LessonStatus
{
    New,
    Progress,
    Done,
    Warn,
    Expired
}