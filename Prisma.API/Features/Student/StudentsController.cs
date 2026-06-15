using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.LessonCatalog.Queries;

namespace Prisma.API.Features.Student;

public class StudentsController(ISender mediator) : ApiController
{
    [Authorize(Roles = AppClaims.Roles.Student)]
    [ProducesResponseType<Result<ICollection<LessonCatalogDto>>>(StatusCodes.Status200OK)]
    [HttpGet("catalog")]
    public async Task<ActionResult> GetLessonsCatalog(CancellationToken c)
    {
        var result = await mediator.Send(new GetLessonsCatalogQuery(), c);
        return Ok(result);
    }
}