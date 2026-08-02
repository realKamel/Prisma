using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Admin.Queries.GetActivityLogsQuery;

public record GetActivityLogsQuery(int Skip = 0, int Take = 20) : IRequest<Result<ActivityLogResponseDto>>;

public record ActivityLogResponseDto(
    ActivityLogStatsDto? Stats,
    List<ActivityEventDto> Events,
    bool HasMore
);

public record ActivityLogStatsDto(
    int TotalEvents,
    int TodayEvents,
    int ActiveUsers,
    int Alerts
);

public record ActivityEventDto(
    DateTimeOffset CreatedAt,
    string User,
    string Role,
    string Action,
    string TableName,
    string EntityId,
    string? Detail
);