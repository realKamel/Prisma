using Ardalis.Result;
using MediatR;
using Prisma.Application.Common.DTOs;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonDetails;

public record GetLessonDetailsQuery(int LessonId) : IRequest<Result<LessonDetailsDto>>;

public record LessonDetailsDto
{
    public int Id { get; init; }
    public string Url { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Teacher { get; init; } = string.Empty;
    public TimeDurationDto Duration { get; init; }
    public int ChaptersCount { get; init; }
    public int StudentsCount { get; init; }
    public MoneyDto Money { get; init; }
    public int ValidityDays { get; init; }
    public DateRangeDto? ValidityDateRange { get; set; }
    public string AboutText { get; init; } = string.Empty;
    public IList<string> Outcomes { get; init; } = [];
    public IList<PrerequisiteDto> Prerequisites { get; init; } = [];
    public IList<ChapterDto> Chapters { get; init; } = [];
}

public record PrerequisiteDto(string Title, bool IsDone);

public record ChapterDto(int Id, string Title, TimeDurationDto Duration, bool IsPreview);
