using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Prisma.Application.Features.Teachers.Queries.GetTeacherDashboardStatus;
using Prisma.Application.Features.Teachers.Queries.GetTeacherFinances;
using Prisma.Application.Features.Teachers.Queries.GetTeacherLessons;

namespace Prisma.API.Features.Teacher;

public class TeachersController(ISender mediator) : ApiController
{
    [HttpGet("dashboard")]
    [ProducesResponseType<Result<GetTeacherDashboardStatusResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<Result<GetTeacherDashboardStatusResponse>> GetTeacherDashboardStatus(CancellationToken token)
    {
        var result = await mediator.Send(new GetTeacherDashboardStatusQuery(), token);
        return result;
    }

    [HttpGet("lessons")]
    [ProducesResponseType<Result<List<TeacherLessonDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<Result<List<TeacherLessonDto>>> GetTeacherLessons(CancellationToken token)
    {
        var result = await mediator.Send(new GetTeacherLessonsQuery(), token);
        return result;
    }

    [HttpGet("finances")]
    [ProducesResponseType<Result<List<RawTransactionDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<Result<List<RawTransactionDto>>> GetTeacherFinances(CancellationToken token)
    {
        var result = await mediator.Send(new GetTeacherFinancesQuery(), token);
        return result;
    }
}