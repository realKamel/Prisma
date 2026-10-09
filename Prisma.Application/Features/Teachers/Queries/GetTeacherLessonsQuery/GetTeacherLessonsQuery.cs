using Ardalis.Result;
using MediatR;
using Prisma.Application.Common.DTOs;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherLessonsQuery;

public record GetTeacherLessonsQuery(PaginationParams PaginationParams)
    : IRequest<Result<PaginatedList<TeacherLessonDto>>>;

public record TeacherLessonDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public MoneyDto Money { get; init; }
    public decimal Price { get; init; }
    public int Students { get; init; }
    public LessonStatus Status { get; init; }
}
