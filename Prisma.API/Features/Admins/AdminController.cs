using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Ardalis.Result;
using Prisma.Application.Features.Admin.Queries.GetActivityLogsQuery;
using Prisma.Application.Features.Admin.Queries.GetAdminActivitiesQuery;
using Prisma.Application.Features.Admin.Queries.GetAdminStatsQuery;

namespace Prisma.API.Features.Admin;

public class AdminController(IMediator mediator) : ApiController
{
    [HttpGet("activity-logs")]
    public async Task<Result<ActivityLogResponseDto>> GetActivityLogs([FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        var result = await mediator.Send(new GetActivityLogsQuery(skip, take));
        return result;
    }


    [HttpGet("stats")]
    [ProducesResponseType(typeof(Result<AdminStatsResponseDto>), StatusCodes.Status200OK)]
    public async Task<Result<AdminStatsResponseDto>> GetStats()
    {
        var query = new GetAdminStatsQuery();
        var result = await mediator.Send(query);
        return result;
    }


    [HttpGet("activities")]
    [ProducesResponseType(typeof(Result<List<AdminActivityDto>>), StatusCodes.Status200OK)]
    public async Task<Result<List<AdminActivityDto>>> GetActivities()
    {
        var query = new GetAdminActivitiesQuery();
        var result = await mediator.Send(query);
        return result;
    }
}