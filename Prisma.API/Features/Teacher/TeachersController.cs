using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.AdminTeachers.Queries.GetTeachers;
using Prisma.Application.Features.Teachers.Commands.ActivateTeacherCommand;
using Prisma.Application.Features.Teachers.Commands.SuspendTeacherCommand;
using Prisma.Application.Features.Teachers.Queries.GetTeacherDashboardStatus;
using Prisma.Application.Features.Teachers.Queries.GetTeacherFinances;
using Prisma.Application.Features.Teachers.Queries.GetTeacherLessons;
using Prisma.Application.Features.Teachers.Queries.GetTeachers;
using Prisma.Application.Features.Teachers.Queries.GetTeacherStatsQuery;

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

    [HttpGet("stats")]
    [ProducesResponseType<Result<TeacherStatsDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<Result<TeacherStatsDto>> GetTeacherStats(CancellationToken token)
    {
        var result = await mediator.Send(new GetTeacherStatsQuery(), token);
        return result;
    }

    [HttpGet]
    [ProducesResponseType<Result<List<TeacherDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<Result<List<TeacherDto>>> GetTeachers(CancellationToken token)
    {
        var result = await mediator.Send(new Application.Features.Teachers.Queries.GetTeachers.GetTeachersQuery(), token);
        return result;
    }

  
    [HttpPut("{id:guid}/activate")]
    [ProducesResponseType<Result<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<Result<bool>> ActivateTeacher(Guid id, CancellationToken token)
    {
        var result = await mediator.Send(new ActivateTeacherCommand(id), token);
        return result;
    }

  
    [HttpPut("{id:guid}/suspend")]
    [ProducesResponseType<Result<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<Result<bool>> SuspendTeacher(
        Guid id,
        [FromBody] SuspendTeacherRequest request,
        CancellationToken token)
    {
        var result = await mediator.Send(new SuspendTeacherCommand(id, request.Reason), token);
        return result;
    }
}