using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Students.Queries.GetStudentHistoryQuery;

public record GetStudentHistoryQuery() : IRequest<Result<GetStudentHistoryResponse>>;

public record GetStudentHistoryResponse(Status status, List<History> history);

public record History(
    int LessonId,
    string? ImageUrl,
    string Title,
    string Status,
    DateTimeOffset? PurchaseDate,
    DateTimeOffset? FinishAt,
    DateTimeOffset? ExpiresAt,
    decimal? QuizDegree,
    int? LessonPercentage);

public record Status(
    int TotalPurchasedLessons,
    int CompletedLessonsCount,
    int TotalStudyCount,
    decimal AverageQuizDegree);