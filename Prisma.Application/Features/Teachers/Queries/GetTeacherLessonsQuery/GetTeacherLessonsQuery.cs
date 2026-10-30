using Ardalis.Result;
using MediatR;
using Prisma.Application.Common.DTOs;
using Prisma.Domain.Enums;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherLessonsQuery;

public record GetTeacherLessonsQuery(
    PaginationParams PaginationParams,
    string? Search = null,
    LessonStatus? Status = null,
    TeacherLessonSort Sort = TeacherLessonSort.Newest
) : IRequest<Result<TeacherLessonsResponse>>;

public class TeacherLessonsRequest
{
    public PaginationParams Pagination { get; set; } = new();
    public string? Search { get; set; }
    public LessonStatus? Status { get; set; }
    public TeacherLessonSort Sort { get; set; } = TeacherLessonSort.Newest;
}
/// <summary>One page of lessons plus per-status totals (ignoring search/status filters).</summary>
public record TeacherLessonsResponse(
    PaginatedList<TeacherLessonDto> Lessons,
    TeacherLessonStatusCounts Counts
);

public record TeacherLessonStatusCounts(int All, int Active, int Hidden, int Drafted);

public record TeacherLessonDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public MoneyDto Money { get; init; }
    public decimal Price { get; init; }
    public int Students { get; init; }
    public LessonStatus Status { get; init; }
    public string? ThumbnailUrl { get; init; }
    public int SectionsCount { get; init; }
    public int DurationSeconds { get; init; }
    public int? CompletionRate { get; init; }
    public decimal Revenue { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}