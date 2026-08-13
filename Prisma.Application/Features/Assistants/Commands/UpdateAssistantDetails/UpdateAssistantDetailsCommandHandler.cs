using System.Security.Claims;
using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Assistants.Commands.UpdateAssistantDetails;

public class UpdateAssistantDetailsCommandHandler(IUnitOfWork uow, IIdentityService identityService) : IRequestHandler<UpdateAssistantDetailsCommand, Result<UpdateAssistantDetailsCommandResponse>>
{
    public async Task<Result<UpdateAssistantDetailsCommandResponse>> Handle(UpdateAssistantDetailsCommand request, CancellationToken cancellationToken)
    {
        var assistantRepository = uow.GetOrCreateRepository<Assistant, Guid>();

        var assistant = await assistantRepository.GetByIdAsync(request.Id, cancellationToken);

        if (assistant is null)
        {
            return Result.NotFound($"Assistant with Id {request.Id} not found.");
        }

        assistant.FirstName = request.FirstName;
        assistant.SecondName = request.SecondName;
        assistant.UserName = request.Email;
        assistant.Email = request.Email;
        assistant.PhoneNumber = request.PhoneNumber;

        await identityService.SetPhoneNumberAsync(assistant, request.PhoneNumber);
        await identityService.SetUserNameAsync(assistant, request.Email);
        await identityService.SetEmailAsync(assistant, request.Email);

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var token = await identityService.GeneratePasswordResetTokenAsync(assistant);
            await identityService.ResetPasswordAsync(assistant, token, request.Password);
        }

        var permissions = request
            .Policies
            .Where(p => AppClaims.Policies.All.Contains(p))
            .Select(px => new Claim(AppClaims.PermissionsClaim, px));

        var claimsResult = await identityService.AddClaimsAsync(assistant, permissions);

        if (!claimsResult.Succeeded)
        {
            return Result.Error(string.Join("\n", claimsResult.Errors.Select(e => e.Code)));
        }

        var dbResult = await uow.SaveChangesAsync(cancellationToken);

        return new UpdateAssistantDetailsCommandResponse(assistant.Id, assistant.Email, assistant.FirstName, assistant.SecondName, assistant.PhoneNumber, request.Policies);
    }
}