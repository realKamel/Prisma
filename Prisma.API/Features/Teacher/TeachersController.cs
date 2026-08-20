using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.DTOs;
using Prisma.Application.Features.Students.Queries.GetLessonsCatalog;
using Prisma.Application.Features.Students.Queries.GetPagedTeacherLessonsCatalog;
using Prisma.Application.Features.Teachers.Commands.ActivateTeacherCommand;
using Prisma.Application.Features.Teachers.Commands.SuspendTeacherCommand;
using Prisma.Application.Features.Teachers.Queries.GetPublicTeacherProfile;
using Prisma.Application.Features.Teachers.Queries.GetTeacherDashboardStatus;
using Prisma.Application.Features.Teachers.Queries.GetTeacherFinances;
using Prisma.Application.Features.Teachers.Queries.GetTeacherLessonsQuery;
using Prisma.Application.Features.Teachers.Queries.GetTeachersQuery;
using Prisma.Application.Features.Teachers.Queries.GetTeacherStatsQuery;

namespace Prisma.API.Features.Teacher;

public class TeachersController(ISender mediator) : ApiController
{
    [HttpGet("dashboard")]
    [ExpectedFailures(ResultStatus.CriticalError)]
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<Result<GetTeacherDashboardStatusResponse>> GetTeacherDashboardStatus(
        CancellationToken token
    )
    {
        var result = await mediator.Send(new GetTeacherDashboardStatusQuery(), token);
        return result;
    }

    [HttpGet("lessons")]
    [ExpectedFailures(ResultStatus.CriticalError, ResultStatus.Unauthorized)]
    public async Task<Result<List<TeacherLessonDto>>> GetTeacherLessons(CancellationToken token)
    {
        var result = await mediator.Send(new GetTeacherLessonsQuery(), token);
        return result;
    }

    [HttpGet("finances")]
    [ProducesResponseType<Result<List<RawTransactionDto>>>(StatusCodes.Status200OK)]
    [ExpectedFailures(ResultStatus.CriticalError, ResultStatus.Unauthorized)]
    public async Task<Result<List<RawTransactionDto>>> GetTeacherFinances(CancellationToken token)
    {
        var result = await mediator.Send(new GetTeacherFinancesQuery(), token);
        return result;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<List<TeacherDto>>), StatusCodes.Status200OK)]
    public async Task<Result<List<TeacherDto>>> GetTeachers()
    {
        var result = await mediator.Send(new GetTeachersQuery());
        return result;
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(Result<TeacherStatsDto>), StatusCodes.Status200OK)]
    public async Task<Result<TeacherStatsDto>> GetTeacherStats()
    {
        var result = await mediator.Send(new GetTeacherStatsQuery());
        return result;
    }

    [HttpPut("{id:guid}/activate")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<Result<bool>> ActivateTeacher(Guid id)
    {
        var result = await mediator.Send(new ActivateTeacherCommand(id));
        return result;
    }

    [HttpPut("{id:guid}/suspend")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<Result<bool>> SuspendTeacher(
        Guid id,
        [FromBody] SuspendTeacherRequest request
    )
    {
        var result = await mediator.Send(new SuspendTeacherCommand(id, request.Reason));
        return result;
    }

    [HttpGet("{id:guid}/lessons")]
    public async Task<Result<PaginatedList<LessonCatalogDto>>> GetLessons(
        [FromRoute] Guid id,
        [FromQuery] string? keyword,
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(
            new GetPaginatedTeacherLessonsQuery(id, keyword, pagination),
            cancellationToken
        );
    }

    [HttpGet("{id:guid}/profile")]
    public async Task<Result<PublicTeacherProfileResponse>> GetTeacherProfile(
        [FromRoute] Guid id,
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(new GetPublicTeacherProfileQuery(id), cancellationToken);
    }
}
