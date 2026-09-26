using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonPlayer;

public record GetLessonPlayerQuery(int Id) : IRequest<Result<LessonPlayerResult>>;

public record LessonPlayerResult
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public Guid EnrollmentId { get; init; }
    public string Description { get; init; } = string.Empty;
    public string Teacher { get; init; } = string.Empty;

    public int ValidityDays { get; init; }

    public string VideoPoster { get; init; } = string.Empty;
    public IList<MaterialDto> Materials { get; init; } = [];
    public QuizDto? Quiz { get; init; }
    public AssignmentDto? Assignment { get; init; }
    public IList<SectionDto> Sections { get; init; } = [];

    public IList<string> Outcomes { get; init; } = [];
}

public record MaterialDto
{
    public string Title { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string DownloadUrl { get; init; } = string.Empty;
}

public record QuizDto
{
    public int Id { get; init; }
    public int QuestionsCount { get; init; }
    public int DurationMinutes { get; init; }
    public int PassingScore { get; init; }
    public bool IsAttempted { get; init; }
}

public record AssignmentDto
{
    public int Id { get; init; }
    public string ContentURL { get; init; } = string.Empty;
    public DateTimeOffset DueDate { get; init; }
    public string FileName { get; init; } = string.Empty;
}

public record SectionDto
{
    public int Id { get; init; }
    public int SectionId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Duration { get; init; } = string.Empty;
    public bool IsCompleted { get; init; }
    public string? ContentUrl { get; init; }
    public int Progress { get; init; }
    public double WatchedSeconds { get; init; }
}