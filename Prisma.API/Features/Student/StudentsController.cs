using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.LessonCatalog.Queries;
using Prisma.Application.Features.Students.Queries.GetStudentHistoryQuery;

namespace Prisma.API.Features.Student;

public class StudentsController(ISender mediator) : ApiController
{
    [Authorize]
    [HttpGet("catalog")]
    public async Task<IActionResult> GetLessonsCatalog(CancellationToken c)
    {
        var result = await mediator.Send(new GetLessonsCatalogQuery(), c);
        return Ok(result);
    }

    // [Authorize(Roles = AppClaims.Roles.Student)]
    [HttpGet("history")]
    [ProducesResponseType<Result<GetStudentHistoryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<GetStudentHistoryResponse>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<GetStudentHistoryResponse>>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetStudentHistory(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStudentHistoryQuery(), cancellationToken);
        return Ok(result);
    }
}