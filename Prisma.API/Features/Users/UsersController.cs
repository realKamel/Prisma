using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.API.Features.Users.Requests;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Users.Commands.CreateUser;
using Prisma.Application.Features.Users.Commands.DeleteUser;
using Prisma.Application.Features.Users.Commands.UpdateUser;
using Prisma.Application.Features.Users.Dtos;
using Prisma.Application.Features.Users.Queries.GetAdminProfile;
using Prisma.Application.Features.Users.Queries.GetAllUsers;
using Prisma.Application.Features.Users.Queries.GetAssistantProfile;
using Prisma.Application.Features.Users.Queries.GetTeacherOptions;
using Prisma.Application.Features.Users.Queries.GetTeacherProfile;
using Prisma.Application.Features.Users.Queries.GetUserById;

namespace Prisma.API.Features.Users;

[Authorize(Roles = AppRoles.Admin)]
public class UsersController(ISender mediator) : ApiController
{
    [HttpGet]
    [ProducesResponseType<Result<List<UserListItemDto>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllUsersQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<Result<UserEditDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUserByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/teacher-dashboard")]
    [ProducesResponseType<Result<RoleProfileDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetTeacherDashboard(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTeacherProfileQuery(id), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/assistant-dashboard")]
    [ProducesResponseType<Result<RoleProfileDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAssistantDashboard(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAssistantProfileQuery(id), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/admin-dashboard")]
    [ProducesResponseType<Result<RoleProfileDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAdminDashboard(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAdminProfileQuery(id), ct);
        return Ok(result);
    }

    [HttpGet("teachers")]
    [ProducesResponseType<Result<List<TeacherOptionDto>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetTeacherOptions(CancellationToken ct)
    {
        var result = await mediator.Send(new GetTeacherOptionsQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var command = new CreateUserCommand(
            request.FirstName, request.SecondName, request.ThirdName, request.LastName,
            request.Mobile, request.Email, request.Password, request.Role,
            request.GradeId, request.TeacherId, request.ParentMobile);

        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var command = new UpdateUserCommand(
            id, request.FirstName, request.SecondName, request.ThirdName, request.LastName,
            request.Mobile, request.Email, request.NewPassword,
            request.GradeId, request.TeacherId, request.ParentMobile);

        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteUserCommand(id), ct);
        return Ok(result);
    }
}