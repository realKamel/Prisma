using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Assistants.Commands.CreateAssistant;
using Prisma.Application.Features.Assistants.Commands.DeleteAssistant;
using Prisma.Application.Features.Assistants.Commands.UpdateAssistantDetails;
using Prisma.Application.Features.Assistants.Commands.UpdatePermissions;
using Prisma.Application.Features.Assistants.Dtos;
using Prisma.Application.Features.Assistants.Queries.GetAssistantDashboard;
using Prisma.Application.Features.Assistants.Queries.GetAssistantLessons;
using Prisma.Application.Features.Assistants.Queries.GetAssistants;

namespace Prisma.API.Features.Assistant;

public class AssistantsController(ISender mediator) : ApiController
{
    [HttpGet]
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<Result<List<AssistantInfo>>> GetAssistants(CancellationToken ctx)
    {
        return await mediator.Send(new GetAssistantQuery(), ctx);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<Result<CreateOrUpdatedAssistantCommandResponse>> CreateAssistant(CreateAssistantCommand command,
        CancellationToken ctx)
    {
        return await mediator.Send(command, ctx);
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
        return await mediator.Send(new UpdatePermissionCommand(id, permissions), cancellationToken);
    }

    [HttpPut("{id}")]
    public async Task<Result<UpdateAssistantDetailsCommandResponse>> UpdateAssistantDetails([FromRoute] Guid id, UpdateAssistantDetailsCommand UpdateAssistantDetails, CancellationToken cancellationToken)
    {
        var command = UpdateAssistantDetails with { Id = id };
        return await mediator.Send(command, cancellationToken);
    }

    [HttpGet("lessons")]
    [ExpectedFailures(ResultStatus.Unauthorized, ResultStatus.Error)]
    public async Task<Result<List<AssistantLessonDto>>> GetAssistantLessons(CancellationToken token)
    {
        return await mediator.Send(new GetAssistantLessonsQuery(), token);
    }

    [HttpGet("dashboard")]
    public async Task<Result<GetAssistantDashboardResponse>> GetAssistantDashboard(CancellationToken ctx)
    {
        return await mediator.Send(new GetAssistantDashboardQuery(), ctx);
    }

    [HttpGet("detailed-logs")]
    [ExpectedFailures(ResultStatus.Unauthorized, ResultStatus.Error)]
    public async Task<Result<GetAssistantDetailedLogsResponseDto>> GetAssistantDetailedLogs([FromQuery] int take,
        CancellationToken token)
    {
        var query = new GetAssistantDetailedLogsQuery(take);
        return await mediator.Send(query, token);
    }
}