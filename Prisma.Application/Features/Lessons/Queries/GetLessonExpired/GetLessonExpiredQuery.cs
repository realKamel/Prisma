using Ardalis.Result;
using MediatR;
using Prisma.Application.Common.DTOs;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonExpired;

public record GetLessonExpiredQuery(Guid LessonId) : IRequest<Result<LessonExpiredDto>>;

public record LessonExpiredDto
{
    public Guid Id { get; init; }
    public string Url { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public int ChaptersCount { get; init; }
    public decimal Price { get; init; }

    public MoneyDto MoneyDto { get; init; }

    public int MaterialsCount { get; init; }

    public double totalprogress { get; init; }

    public decimal Degree { get; init; }

    public DateTimeOffset? ExpiredDate { get; init; }
    public int ValidityDays { get; init; }
    public IList<ChapterDto> Chapters { get; init; } = [];
}

public record ChapterDto(int Id, string Title, TimeDurationDto Duration);
