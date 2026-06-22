using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Teachers.Queries.GetTeacherDashboardStatus;

namespace Prisma.API.Features.Teacher;

public class TeachersController(ISender mediator) : ApiController
{
    [HttpGet("dashboard")]
    [ProducesResponseType<Result<GetTeacherDashboardStatusResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetTeacherDashboardStatus(CancellationToken token)
    {
        var result = await mediator.Send(new GetTeacherDashboardStatusQuery(), token);
        return Ok(result);
    }
}