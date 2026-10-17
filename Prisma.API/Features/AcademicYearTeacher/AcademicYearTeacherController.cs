using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.AcademicYears.Dtos;
using Prisma.Application.Features.AcademicYears.Queries.GetAllAcademicYears;
using Prisma.Application.Features.Teachers.Queries.GetTeacherDashboardStatus;

namespace Prisma.API.Features.AcademicYearTeacher;


public class AcademicYearTeacherController(ISender mediator) : ApiController
{
    [HttpGet("academic-year-options")]
    public async Task<Result<List<AcademicYearOptionDto>>> GetAcademicYearTeacher(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllAcademicYearsQuery(), cancellationToken);
        return result;
    }
}
