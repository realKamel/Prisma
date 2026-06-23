using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.LessonCatalog.Queries;
using Prisma.Application.Features.Students.Queries.GetStudentDashboardQuery;
using Prisma.Application.Features.Students.Queries.GetStudentHistoryQuery;

namespace Prisma.API.Features.Student;

public class StudentsController(ISender mediator) : ApiController
{
    [Authorize(Roles = AppRoles.Student)]
    [ProducesResponseType<Result<ICollection<LessonCatalogDto>>>(StatusCodes.Status200OK)]
    [HttpGet("catalog")]
    public async Task<ActionResult> GetLessonsCatalog(CancellationToken c)
    {
        var result = await mediator.Send(new GetLessonsCatalogQuery(), c);
        return Ok(result);
    }

    // [Authorize(Roles = AppRoles.Student)]
    [HttpGet("history")]
    [ProducesResponseType<Result<GetStudentHistoryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<GetStudentHistoryResponse>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<GetStudentHistoryResponse>>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetStudentHistory(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStudentHistoryQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("dashboard")]
    [ProducesResponseType<Result<GetStudentDashboardResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<GetStudentDashboardResponse>>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetStudentDashboard(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStudentDashboardQuery(), cancellationToken);
        return Ok(result);
    }
}