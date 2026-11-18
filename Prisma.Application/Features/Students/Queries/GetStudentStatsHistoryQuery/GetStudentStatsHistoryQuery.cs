using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Students.Queries.GetStudentStatsHistoryQuery;

public record GetStudentStatsHistoryQuery : IRequest<Result<StatsHistoryDto>>;

public record StatsHistoryDto
{
    public List<StatPointDto> PurchasedLessons { get; init; } = [];
    public List<StatPointDto> CompletedLessons { get; init; } = [];
    public List<StatPointDto> StudyHours { get; init; } = [];

    public List<StatPointDto> TopQuizScore { get; init; } = [];
}

public record StatPointDto(DateOnly Date, double Value);