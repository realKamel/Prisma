using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Assistants.Queries.GetAssistantDetailedLogs;

public record GetAssistantDetailedLogsQuery(int Take) : IRequest<Result<GetAssistantDetailedLogsResponseDto>>;

public record GetAssistantDetailedLogsResponseDto(
    DashboardMetaDto Meta,
    IList<DetailedLogItemDto> Logs
);

public record DashboardMetaDto(
    int TotalThisMonth,
    int Granted,
    int Revoked,
    int SuccessRate
);

public record DetailedLogItemDto(
    int Id,
    string Type,
    string Detail,
    string Sub,
    string Student,
    string Grade,
    DateTimeOffset Time,
    bool Ok
);