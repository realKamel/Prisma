using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.ActivityLogs.Queries.GetActivityLogs;
using Prisma.Application.Features.AdminDashboard.Queries.GetAdminActivities;
using Prisma.Application.Features.AdminDashboard.Queries.GetAdminStats;

namespace Prisma.API.Features.Admin;

public class AdminController(IMediator mediator) : ApiController
{
    [HttpGet("activity-logs")]
    public async Task<IActionResult> GetActivityLogs([FromQuery] int take = 20, [FromQuery] string role = "all")
    {
        var query = new GetActivityLogsQuery(take, role);
        var result = await mediator.Send(query);

        if (!result.Succeeded)
            return BadRequest(result);

        return Ok(result.Data);
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
