using System;
using System.Collections.Generic;
using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.ActivityLogs.Queries.GetActivityLogs;

public record GetActivityLogsQuery(int Take = 20, string Role = "all") : IRequest<Result<ActivityLogResponseDto>>;

public record ActivityLogResponseDto(
    ActivityLogStatsDto Stats,
    List<ActivityEventDto> Events
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
    string EntityId
);