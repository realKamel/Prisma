using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prisma.API.Common;
using Prisma.API.Common.RateLimitConfigurations;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.DTOs;
using Prisma.Application.Features.Students.Commands.ChangePasswordCommand;
using Prisma.Application.Features.Students.Commands.FinishPractice;
using Prisma.Application.Features.Students.Commands.HandleStreaks;
using Prisma.Application.Features.Students.Commands.SubmitPracticeAnswer;
using Prisma.Application.Features.Students.Commands.UpdateStudentProfileCommand;
using Prisma.Application.Features.Students.Common;
using Prisma.Application.Features.Students.Queries.GetPaginatedLessonsCatalog;
using Prisma.Application.Features.Students.Queries.GetPracticeRound;
using Prisma.Application.Features.Students.Queries.GetStreakTasks;
using Prisma.Application.Features.Students.Queries.GetStudentDashboardQuery;
using Prisma.Application.Features.Students.Queries.GetStudentHistoryQuery;
using Prisma.Application.Features.Students.Queries.GetStudentPaymentHistory;
using Prisma.Application.Features.Students.Queries.GetStudentPerformanceStatus;
using Prisma.Application.Features.Students.Queries.GetStudentProfileQuery;
using Prisma.Application.Features.Students.Queries.GetStudentStatsHistoryQuery;
using Prisma.Application.Features.Students.Queries.GetTeacherCatalog;

namespace Prisma.API.Features.Student;

public sealed class StudentsController(ISender mediator) : ApiController
{
    [Authorize(Roles = AppRoles.Student)]
    [HttpGet("catalog")]
    public async Task<Result<PaginatedList<LessonCatalogDto>>> GetLessonsCatalog([FromQuery] string? keyword,
        [FromQuery] PaginationParams paginationParams, CancellationToken cancellationToken)
    {
        return await mediator.Send(new GetPaginatedLessonsCatalogQuery(paginationParams), cancellationToken);
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
    [HttpGet("dashboard/stats-history")]
    [ExpectedFailures(ResultStatus.CriticalError, ResultStatus.Error, ResultStatus.NotFound)]
    public async Task<Result<StatsHistoryDto>> GetStatsHistory(CancellationToken ct)
    {
        var result = await mediator.Send(new GetStudentStatsHistoryQuery(), ct);
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
    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
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
    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
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
    [Authorize(Roles = AppRoles.Student)]
    [HttpPost("streak")]
    [ExpectedFailures(ResultStatus.Unauthorized, ResultStatus.NotFound)]
    public async Task<Result> HandlerStreak(CancellationToken ct)
    {
        var result = await mediator.Send(new HandlerStreaksCommand(), ct);
        return result;
    }

    [Authorize(Roles = AppRoles.Student)]
    [HttpGet("streak-tasks")]
    [ExpectedFailures(ResultStatus.CriticalError, ResultStatus.Error, ResultStatus.NotFound)]
    public async Task<Result<StreakTasksDto>> GetStreakTasks(CancellationToken ct)
    {
        var res = await mediator.Send(new GetStreakTasksQuery(), ct);
        return res;
    }

    [Authorize(Roles = AppRoles.Student)]
    [HttpGet("practice")]
    [ExpectedFailures(ResultStatus.CriticalError, ResultStatus.Error)]
    public async Task<Result<PracticeRoundDto>> GetPractice(CancellationToken ct) 
    {
        var res = await mediator.Send(new GetPracticeRoundQuery(), ct);
        return res;
    }
    [Authorize(Roles = AppRoles.Student)]
    [HttpPost("practice/finish")]
    [ExpectedFailures(ResultStatus.CriticalError, ResultStatus.Error)]
    public async Task<Result<FinishPracticeResultDto>> FinishPractice(CancellationToken ct) 
    {
        var res = await mediator.Send(new FinishPracticeCommand(), ct);
        return res;
    }

    [Authorize(Roles = AppRoles.Student)]
    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    [HttpPost("practice/answer")]
    [ExpectedFailures(ResultStatus.CriticalError, ResultStatus.Error, ResultStatus.NotFound)]
    public async Task<Result<PracticeAnswerResultDto>> AnswerPractice(
        [FromBody] SubmitPracticeAnswerCommand command, CancellationToken ct)
    {
        var res = await mediator.Send(command, ct);
        return res;
    }

    [HttpGet("teachers")]
    [ExpectedFailures(ResultStatus.CriticalError)]
    public async Task<Result<TeacherCatalogResponse>> GetTeachers(
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