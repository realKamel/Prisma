using System;
using System.Collections.Generic;
using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.AdminDashboard.Queries.GetAdminActivities;

public record GetAdminActivitiesQuery() : IRequest<Result<List<AdminActivityDto>>>;

public record AdminActivityDto(
    string Id,
    string Type,         
    string EntityId,    
    string Details,     
    string MetaInfo,    
    DateTimeOffset ActivityDate 
);