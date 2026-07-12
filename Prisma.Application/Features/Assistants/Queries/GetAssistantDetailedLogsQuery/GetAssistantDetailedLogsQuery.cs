using MediatR;
using Prisma.Application.Common.Responses.Generic;

public record GetAssistantDetailedLogsQuery(int Take) : IRequest<Result<GetAssistantDetailedLogsResponseDto>>;

public record GetAssistantDetailedLogsResponseDto(
    DashboardMetaDto Meta,
    List<DetailedLogItemDto> Logs
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
    string Time,
    string Date,
    bool Ok
);