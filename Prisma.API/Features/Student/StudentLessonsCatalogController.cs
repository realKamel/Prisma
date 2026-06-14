using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.LessonCatalog.Queries;

namespace Prisma.API.Features.Student;

public class StudentLessonsCatalogController : ApiController
{
    private readonly IMediator _mediator;

    public StudentLessonsCatalogController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize (Roles = AppClaims.Roles.Student) ]
    [HttpGet("catalog")]
    public async Task<IActionResult> GetLessonsCatalog(CancellationToken c)
    {
        var result = await _mediator.Send(new GetLessonsCatalogQuery(), c);
        return Ok(result);
    }
}