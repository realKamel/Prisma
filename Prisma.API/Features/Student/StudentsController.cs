using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Prisma.Application.Features.LessonCatalog.Queries;
using Prisma.Application.Features.Students.Commands.ChangePasswordCommand;
using Prisma.Application.Features.Students.Commands.UpdateStudentProfileCommand;
using Prisma.Application.Features.Students.Queries.GetStudentDashboardQuery;
using Prisma.Application.Features.Students.Queries.GetStudentHistoryQuery;
using Prisma.Application.Features.Students.Queries.GetStudentPaymentHistory;
using Prisma.Application.Features.Students.Queries.GetStudentProfileQuery;

namespace Prisma.API.Features.Student;

public class StudentsController(ISender mediator) : ApiController
{
    [Authorize(Roles = AppRoles.Student)]
    [ProducesResponseType<Result<ICollection<LessonCatalogDto>>>(StatusCodes.Status200OK)]
    [HttpGet("catalog")]
    public async Task<Result<ICollection<LessonCatalogDto>>> GetLessonsCatalog(CancellationToken c)
    {
        var result = await mediator.Send(new GetLessonsCatalogQuery(), c);
        return result;
    }

    [Authorize(Roles = AppRoles.Student)]
    [HttpGet("history")]
    [ProducesResponseType<Result<GetStudentHistoryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<GetStudentHistoryResponse>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<GetStudentHistoryResponse>>(StatusCodes.Status500InternalServerError)]
    public async Task<Result<GetStudentHistoryResponse>> GetStudentHistory(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStudentHistoryQuery(), cancellationToken);
        return result;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType<Result<GetStudentDashboardResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<GetStudentDashboardResponse>>(StatusCodes.Status500InternalServerError)]
    public async Task<Result<GetStudentDashboardResponse>> GetStudentDashboard(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStudentDashboardQuery(), cancellationToken);
        return result;
    }

    [Authorize(Roles = AppRoles.Student)]
    [HttpGet("profile")]
    [ProducesResponseType<Result<StudentProfileDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<StudentProfileDto>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Result<StudentProfileDto>>(StatusCodes.Status500InternalServerError)]
    public async Task<Result<StudentProfileDto>> GetProfile(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStudentProfileQuery(), cancellationToken);
        return result;
    }

    [Authorize(Roles = AppRoles.Student)]
    [HttpPut("profile")]
    [ProducesResponseType<Result<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<bool>>(StatusCodes.Status500InternalServerError)]
    public async Task<Result<bool>> UpdateProfile([FromBody] UpdateStudentProfileCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result;
    }

    [Authorize(Roles = AppRoles.Student)]
    [HttpPost("change-password")]
    [ProducesResponseType<Result<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<bool>>(StatusCodes.Status500InternalServerError)]
    public async Task<Result<bool>> ChangePassword([FromBody] ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result;
    }

    [HttpGet("payments/history")]
    [ProducesResponseType<Result<StudentPaymentHistoryResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<StudentPaymentHistoryResponseDto>>(StatusCodes.Status500InternalServerError)]
    public async Task<Result<StudentPaymentHistoryResponseDto>> GetPaymentHistory(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStudentPaymentHistoryQuery(), cancellationToken);
        return result;
    }
}