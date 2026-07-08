using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.ActivityLogs.Queries.GetActivityLogs;
using Prisma.Application.Features.AdminDashboard.Queries.GetAdminActivities;
using Prisma.Application.Features.AdminDashboard.Queries.GetAdminStats;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Prisma.API.Features.Admin;

public class AdminController(IMediator mediator) : ApiController
{
    [HttpGet("activity-logs")]
    public async Task<IActionResult> GetActivityLogs([FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        var result = await mediator.Send(new GetActivityLogsQuery(skip, take));
        return Ok(result);
    }

    

    [HttpGet("stats")]
    [ProducesResponseType(typeof(Result<AdminStatsResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var query = new GetAdminStatsQuery();
        var result = await mediator.Send(query);
        return Ok(result);
    }

   
    [HttpGet("activities")]
    [ProducesResponseType(typeof(Result<List<AdminActivityDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivities()
    {
        var query = new GetAdminActivitiesQuery();
        var result = await mediator.Send(query);
        return Ok(result);
    }

  
}
