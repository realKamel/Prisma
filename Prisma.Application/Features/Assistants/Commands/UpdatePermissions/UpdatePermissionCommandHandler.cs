using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Prisma.Application.Features.Assistants.Dtos;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.Assistants.Commands.UpdatePermissions;

public class UpdatePermissionCommandHandler(IIdentityService service, UserManager<User> userManager)
    : IRequestHandler<UpdatePermissionCommand, Result<CreateOrUpdatedAssistantCommandResponse>>
{
    async Task<Result<CreateOrUpdatedAssistantCommandResponse>>
        IRequestHandler<UpdatePermissionCommand, Result<CreateOrUpdatedAssistantCommandResponse>>.Handle(
            UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        var assistant = await service.FindByIdAsync(request.Id, cancellationToken);

        if (assistant is null)
        {
            return Result.NotFound($"Assistant with id '{request.Id}' was not found");
        }

        var permissions = request
            .Permission
            .Where(p => AppClaims.Policies.All.Contains(p))
            .Select(px => new Claim(AppClaims.PermissionsClaim, px));

        await userManager.RemoveClaimsAsync(assistant, await userManager.GetClaimsAsync(assistant));
        await userManager.AddClaimsAsync(assistant, permissions);

        return new CreateOrUpdatedAssistantCommandResponse
        (assistant.Id,
            assistant.FirstName,
            assistant.SecondName,
            assistant.PhoneNumber,
            assistant.Email,
            request.Permission.ToList());
    }
}