using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.DTOs;
using Prisma.Application.Features.Students.Commands.ChangePasswordCommand;
using Prisma.Application.Features.Students.Commands.UpdateStudentProfileCommand;
using Prisma.Application.Features.Students.Queries.GetLessonsCatalog;
using Prisma.Application.Features.Students.Queries.GetStudentDashboardQuery;
using Prisma.Application.Features.Students.Queries.GetStudentHistoryQuery;
using Prisma.Application.Features.Students.Queries.GetStudentPaymentHistory;
using Prisma.Application.Features.Students.Queries.GetStudentPerformanceStatus;
using Prisma.Application.Features.Students.Queries.GetStudentProfileQuery;
using Prisma.Application.Features.Students.Queries.GetTeacherCatalog;

namespace Prisma.API.Features.Student;

public class StudentsController(ISender mediator) : ApiController
{
    //[Authorize(Roles = AppRoles.Student)]
    [HttpGet("catalog")]
    public async Task<Result<ICollection<LessonCatalogDto>>> GetLessonsCatalog(CancellationToken c)
    {
        var result = await mediator.Send(new GetLessonsCatalogQuery(), c);
        return result;
    }

    [Authorize(Roles = AppRoles.Student)]
    [HttpGet("performance")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.NotFound,
        ResultStatus.Unauthorized
    )]
    public async Task<Result<StatusDto>> GetStudentPerformanceStatus(
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(new GetStudentPerformanceStatusQuery(), cancellationToken);
    }

    [Authorize(Roles = AppRoles.Student)]
    [HttpGet("history")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.NotFound,
        ResultStatus.Unauthorized
    )]
    public async Task<Result<PaginatedList<HistoryDto>>> GetStudentHistory(
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(
            new GetPaginatedStudentHistoryQuery(pagination),
            cancellationToken
        );
    }

    [HttpGet("dashboard")]
    [ExpectedFailures(ResultStatus.CriticalError, ResultStatus.Error, ResultStatus.NotFound)]
    public async Task<Result<GetStudentDashboardResponse>> GetStudentDashboard(
        CancellationToken cancellationToken
    )
    {
        var result = await mediator.Send(new GetStudentDashboardQuery(), cancellationToken);
        return result;
    }

    [Authorize(Roles = AppRoles.Student)]
    [HttpGet("profile")]
    [ExpectedFailures(ResultStatus.CriticalError, ResultStatus.Error, ResultStatus.NotFound)]
    public async Task<Result<StudentProfileDto>> GetProfile(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStudentProfileQuery(), cancellationToken);
        return result;
    }

    [Authorize(Roles = AppRoles.Student)]
    [HttpPut("profile")]
    [ExpectedFailures(ResultStatus.CriticalError, ResultStatus.Error, ResultStatus.Invalid)]
    public async Task<Result<bool>> UpdateProfile(
        [FromBody] UpdateStudentProfileCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await mediator.Send(command, cancellationToken);
        return result;
    }

    [Authorize(Roles = AppRoles.Student)]
    [HttpPost("change-password")]
    [ExpectedFailures(ResultStatus.CriticalError, ResultStatus.Error, ResultStatus.Invalid)]
    public async Task<Result<bool>> ChangePassword(
        [FromBody] ChangePasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await mediator.Send(command, cancellationToken);
        return result;
    }

    [HttpGet("payments/history")]
    [ExpectedFailures(ResultStatus.CriticalError, ResultStatus.Error)]
    public async Task<Result<StudentPaymentHistoryResponseDto>> GetPaymentHistory(
        CancellationToken cancellationToken
    )
    {
        var result = await mediator.Send(new GetStudentPaymentHistoryQuery(), cancellationToken);
        return result;
    }

    [HttpGet("teachers")]
    [ExpectedFailures(ResultStatus.CriticalError)]
    public async Task<Result<PaginatedList<TeacherDto>>> GetTeachers(
        string? search,
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(
            new GetTeacherCatalogQuery(search, pagination),
            cancellationToken
        );
    }
}
