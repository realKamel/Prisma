using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Features.ActivityLogs.Queries.GetActivityLogs;

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
}
