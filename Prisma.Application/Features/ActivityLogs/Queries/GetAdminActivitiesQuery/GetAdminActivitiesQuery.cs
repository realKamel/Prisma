using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.ActivityLogs.Queries.GetAdminActivitiesQuery;

public record GetAdminActivitiesQuery : IRequest<Result<List<AdminActivityDto>>>;

public record AdminActivityDto(
    string Id,
    string Type,
    string EntityId,
    string Details,
    string MetaInfo,
    DateTimeOffset ActivityDate
);
