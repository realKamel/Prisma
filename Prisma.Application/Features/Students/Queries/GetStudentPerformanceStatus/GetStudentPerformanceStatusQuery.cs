using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Students.Queries.GetStudentPerformanceStatus;

public record GetStudentPerformanceStatusQuery : IRequest<Result<StatusDto>>;

public record StatusDto(
    int TotalPurchasedLessons,
    int CompletedLessonsCount,
    int TotalStudyCount,
    decimal AverageQuizDegree
);
