using Ardalis.Result;
using MediatR;
using Prisma.Application.Common.DTOs;

namespace Prisma.Application.Features.Students.Queries.GetStudentHistoryQuery;

public record GetPaginatedStudentHistoryQuery(PaginationParams PaginationParams)
    : IRequest<Result<PaginatedList<HistoryDto>>>;

public record HistoryDto(
    int? LessonId,
    string? ImageUrl,
    string? Title,
    string? Status,
    string? TeacherName,
    string? Subject,
    DateTimeOffset? PurchaseDate,
    DateTimeOffset? FinishAt,
    DateTimeOffset? ExpiresAt,
    decimal? QuizDegree,
    double? LessonPercentage
);
