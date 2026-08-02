using System;
using System.Collections.Generic;
using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Admin.Queries.GetAdminActivitiesQuery;

public record GetAdminActivitiesQuery() : IRequest<Result<List<AdminActivityDto>>>;

public record AdminActivityDto(
    string Id,
    string Type,
    string EntityId,
    string Details,
    string MetaInfo,
    DateTimeOffset ActivityDate
);