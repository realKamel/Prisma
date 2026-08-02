using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Ardalis.Result;
using Prisma.Application.Features.AcademicYears.Dtos;
using Prisma.Application.Features.AcademicYears.Queries.GetAllAcademicYears;

namespace Prisma.API.Features.Grades;

[AllowAnonymous] 
public class GradesController(ISender mediator) : ApiController
{
    [HttpGet("grade-options")]
    [ProducesResponseType<Result<List<AcademicYearOptionDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<List<AcademicYearOptionDto>>>(StatusCodes.Status500InternalServerError)]
    public async Task<Result<List<AcademicYearOptionDto>>> GetGradeOptions(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllAcademicYearsQuery(), cancellationToken);
        return result;
    }
}