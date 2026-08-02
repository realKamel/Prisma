using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.API.Features.Users.Requests;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Ardalis.Result;
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
    public async Task<Result<List<UserListItemDto>>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllUsersQuery(), ct);
        return result;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<Result<UserEditDto>>(StatusCodes.Status200OK)]
    public async Task<Result<UserEditDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUserByIdQuery(id), ct);
        return result;
    }

    [HttpGet("{id:guid}/teacher-dashboard")]
    [ProducesResponseType<Result<RoleProfileDto>>(StatusCodes.Status200OK)]
    public async Task<Result<RoleProfileDto>> GetTeacherDashboard(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTeacherProfileQuery(id), ct);
        return result;
    }

    [HttpGet("{id:guid}/assistant-dashboard")]
    [ProducesResponseType<Result<RoleProfileDto>>(StatusCodes.Status200OK)]
    public async Task<Result<RoleProfileDto>> GetAssistantDashboard(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAssistantProfileQuery(id), ct);
        return result;
    }

    [HttpGet("{id:guid}/admin-dashboard")]
    [ProducesResponseType<Result<RoleProfileDto>>(StatusCodes.Status200OK)]
    public async Task<Result<RoleProfileDto>> GetAdminDashboard(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAdminProfileQuery(id), ct);
        return result;
    }

    [HttpGet("teachers")]
    [ProducesResponseType<Result<List<TeacherOptionDto>>>(StatusCodes.Status200OK)]
    public async Task<Result<List<TeacherOptionDto>>> GetTeacherOptions(CancellationToken ct)
    {
        var result = await mediator.Send(new GetTeacherOptionsQuery(), ct);
        return result;
    }

    [HttpPost]
    public async Task<Result<UserEditDto>> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var command = new CreateUserCommand(
            request.FirstName, request.SecondName, request.ThirdName, request.LastName,
            request.Mobile, request.Email, request.Password, request.Role,
            request.GradeId, request.TeacherId, request.ParentMobile);

        var result = await mediator.Send(command, ct);
        return result;
    }

    [HttpPut("{id:guid}")]
    public async Task<Result<UserEditDto>> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var command = new UpdateUserCommand(
            id, request.FirstName, request.SecondName, request.ThirdName, request.LastName,
            request.Mobile, request.Email, request.NewPassword,
            request.GradeId, request.TeacherId, request.ParentMobile);

        var result = await mediator.Send(command, ct);
        return result;
    }

    [HttpDelete("{id:guid}")]
    public async Task<Result> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteUserCommand(id), ct);
        return result;
    }
}