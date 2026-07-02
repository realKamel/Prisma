using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.AcademicYears.Dtos;
using Prisma.Application.Features.AcademicYears.Queries.GetAllAcademicYears;

namespace Prisma.API.Features.Grades;

[AllowAnonymous] 
public class GradesController(ISender mediator) : ApiController
{
    [HttpGet("grade-options")]
    [ProducesResponseType<Result<List<AcademicYearOptionDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<List<AcademicYearOptionDto>>>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetGradeOptions(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllAcademicYearsQuery(), cancellationToken);
        return Ok(result);
    }
}