using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Features.AcademicYears.Dtos;
using Prisma.Application.Features.AcademicYears.Queries.GetAllAcademicYears;

namespace Prisma.API.Features.AcademicYearTeacher;

public sealed class AcademicYearTeacherController(ISender mediator) : ApiController
{
    [HttpGet("academic-year-options")]
    public async Task<Result<List<AcademicYearOptionDto>>> GetAcademicYearTeacher(
        CancellationToken cancellationToken
    )
    {
        var result = await mediator.Send(new GetAllAcademicYearsQuery(), cancellationToken);
        return result;
    }
}
