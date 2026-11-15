using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;

namespace Prisma.Application.Features.Authentication.Commands.ChangePassword;

internal sealed class ChangePasswordCommandHandler(
    ICurrentUserService currentUserService,
    IIdentityService identityService
) : ICommandHandler<ChangePasswordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken
    )
    {
        if (currentUserService.UserId is not { } userId)
        {
            return Result.Unauthorized();
        }

        var user = await identityService.FindByIdAsync(userId, true, cancellationToken);
        if (user is null)
        {
            return Result.Unauthorized();
        }

        if (!await identityService.CheckPasswordAsync(user, request.CurrentPassword))
        {
            // The identifier names the field; the front shows friendly wording for it.
            return Result.Invalid(
                new ValidationError(
                    nameof(request.CurrentPassword),
                    "TEACHER_PROFILE_PAGE.ACCOUNT.WRONG_PASSWORD"
                )
            );
        }

        var token = await identityService.GeneratePasswordResetTokenAsync(user);
        var resetResult = await identityService.ResetPasswordAsync(user, token, request.NewPassword);

        if (!resetResult.Succeeded)
        {
            return Result.Invalid(
                resetResult
                    .Errors.Select(e => new ValidationError(nameof(request.NewPassword), e.Description))
                    .ToList()
            );
        }

        return Result.Success(true);
    }
}
