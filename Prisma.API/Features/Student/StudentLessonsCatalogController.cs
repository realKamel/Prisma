using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.LessonCatalog.Queries;

namespace Prisma.API.Features.Student;


public class StudentLessonsCatalogController: ApiController
{
    private readonly IMediator _mediator;

    public StudentLessonsCatalogController(IMediator mediator)
    {
        _mediator = mediator;
    }


    [Authorize]
    //[AllowAnonymous]
    [HttpGet("catalog")]
    public async Task<IActionResult> GetLessonsCatalog(CancellationToken c)
    {

        var query = new GetLessonsCatalogQuery();
        var result = await _mediator.Send(query, c);

        return Ok(result);
    }



}
