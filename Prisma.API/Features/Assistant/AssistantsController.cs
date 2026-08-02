using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Prisma.Application.Features.Assistants.Commands.CreateAssistant;
using Prisma.Application.Features.Assistants.Commands.DeleteAssistant;
using Prisma.Application.Features.Assistants.Commands.UpdatePermissions;
using Prisma.Application.Features.Assistants.Dtos;
using Prisma.Application.Features.Assistants.Queries.GetAssistantDashboard;
using Prisma.Application.Features.Assistants.Queries.GetAssistantLessons;
using Prisma.Application.Features.Assistants.Queries.GetAssistants;

namespace Prisma.API.Features.Assistant;

public class AssistantsController(ISender mediator) : ApiController
{
    // [HttpGet("students")]
    // // [HasPermission(AppClaims.Permissions.ViewStudentProfile)]
    // [Authorize(AppClaims.Policies.CanManageContent)]
    // public async Task<ActionResult> GetStudents()
    // {
    //     await Task.Run(() => Console.WriteLine($"{nameof(GetStudents)}"));
    //     return Ok();
    // }


    [HttpGet]
    [Authorize(Roles = AppRoles.Teacher)]
    [ProducesResponseType<Result<List<AssistantInfo>>>(StatusCodes.Status200OK)]
    public async Task<Result<List<AssistantInfo>>> GetAssistants(CancellationToken ctx)
    {
        var result = await mediator.Send(new GetAssistantQuery(), ctx);
        return result;
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<Result<CreateOrUpdatedAssistantCommandResponse>> CreateAssistant(CreateAssistantCommand command,
        CancellationToken ctx)
    {
        var result = await mediator.Send(command, ctx);
        return result;
    }

    [HttpDelete("{id}")]
    public async Task<Result> DeleteAssistant(Guid id, CancellationToken ctx)
    {
        var result = await mediator.Send(new DeleteAssistantCommand(id), ctx);
        return result;
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<Result<CreateOrUpdatedAssistantCommandResponse>> UpdateAssistantPermissions(Guid id,
        List<string> permissions,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdatePermissionCommand(id, permissions), cancellationToken);
        return result;
    }

    [HttpGet("lessons")]
    [ProducesResponseType<Result<List<AssistantLessonDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<Result<List<AssistantLessonDto>>> GetAssistantLessons(CancellationToken token)
    {
        var result = await mediator.Send(new GetAssistantLessonsQuery(), token);
        return result;
    }

    [HttpGet("dashboard")]
    public async Task<Result<GetAssistantDashboardResponse>> GetAssistantDashboard(CancellationToken ctx)
    {
        var result = await mediator.Send(new GetAssistantDashboardQuery(), ctx);
        return result;
    }

    [HttpGet("detailed-logs")]
    [ProducesResponseType<Result<GetAssistantDetailedLogsResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<Result<GetAssistantDetailedLogsResponseDto>> GetAssistantDetailedLogs([FromQuery] int take,
        CancellationToken token)
    {
        var query = new GetAssistantDetailedLogsQuery(take);
        var result = await mediator.Send(query, token);
        return result;
    }
}