using System.Security.Claims;
using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Assistants.Dtos;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.Assistants.Commands.CreateAssistant;

public class CreateAssistantCommandHandler(IIdentityService identityService,
    ICurrentUserService currentUserService) :
    IRequestHandler<CreateAssistantCommand, Result<CreateOrUpdatedAssistantCommandResponse>>
{
    public async Task<Result<CreateOrUpdatedAssistantCommandResponse>> Handle(CreateAssistantCommand request,
        CancellationToken cancellationToken)
    {
        var user = await identityService.FindByPhoneNumberAsync(request.PhoneNumber, cancellationToken);
        var teacherId = currentUserService.UserId;
        if (teacherId is null)
        {
            return Result.Unauthorized();
        }

        if (user is not null)
        {
            return Result.Conflict("User already exists");
        }
        

        var assistant = new Assistant
        {
            Id = Guid.CreateVersion7(),
            FirstName = request.FirstName,
            SecondName = request.SecondName,
            UserName = request.Email,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            TeacherId = teacherId
        };

        var result = await identityService.CreateAsync(assistant, request.Password);

        // var permissions = request
        //     .Policies
        //     .SelectMany(p => AppClaims.Policies.PermissionMap[p])
        //     .Select(x => new Claim(AppClaims.PermissionsClaim, x));

        var permissions = request
            .Policies
            .Where(p => AppClaims.Policies.All.Contains(p))
            .Select(px => new Claim(AppClaims.PermissionsClaim, px));

        if (!result.Succeeded)
        {
            return Result.Error(string.Join("\n", result.Errors.Select(e => e.Code)));
        }

        var claimsResult = await identityService.AddClaimsAsync(assistant, permissions);

        if (!claimsResult.Succeeded)
        {
            return Result.Error(string.Join("\n", claimsResult.Errors.Select(e => e.Code)));
        }

        var roleResult = await identityService.AddToRoleAsync(assistant, AppRoles.Assistant);

        if (!roleResult.Succeeded)
        {
            return Result.Error(string.Join("\n", roleResult.Errors.Select(e => e.Code)));
        }

        return new CreateOrUpdatedAssistantCommandResponse
        (assistant.Id,
            assistant.FirstName,
            assistant.SecondName,
            assistant.PhoneNumber,
            assistant.Email,
            request.Policies.ToList());
    }
}