using System.Security.Cryptography;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Common.Responses;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Authentication.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler(
    UserManager<User> _userManager,
    IEmailService _emailService)
    : IRequestHandler<ForgotPasswordCommand, Result>
{
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Success("If this email exists, a reset code was sent.");

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        user.ResetPasswordCodeAttemptCount = 0;
        user.PasswordResetCode = code;
        user.PasswordResetCodeExpiry = DateTimeOffset.UtcNow.AddMinutes(10);
        await _userManager.UpdateAsync(user);

        await _emailService.SendAsync(user.Email!, "Reset Password", $"Your code is: {code}");
        return Result.Success("If this email exists, a reset code was sent.");
    }
}

public class ConfirmCodeCommandHandler(
    UserManager<User> _userManager)
    : IRequestHandler<ConfirmCodeCommand, Result>
{
    public async Task<Result> Handle(ConfirmCodeCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) throw new BadRequestException("Code Invalid");
        user.ResetPasswordCodeAttemptCount++;
        await _userManager.UpdateAsync(user);

        if (user.ResetPasswordCodeAttemptCount > 5)
        {
            user.PasswordResetCode = null;
            user.PasswordResetCodeExpiry = null;
            await _userManager.UpdateAsync(user);
            throw new BadRequestException("Code Invalid");
        }

        if (user.PasswordResetCodeExpiry is null || DateTimeOffset.UtcNow >= user.PasswordResetCodeExpiry)
            throw new BadRequestException("Code Invalid");
        if (user.PasswordResetCode is null || user.PasswordResetCode != request.Code)
            throw new BadRequestException("Code Invalid");

        user.PasswordResetCode = null;
        user.PasswordResetCodeExpiry = null;
        user.PasswordResetConfirmed = true;
        await _userManager.UpdateAsync(user);

        return Result.Success();
    }
}

public class ResetPasswordCommandHandler(
    UserManager<User> _userManager)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) throw new BadRequestException("something went wrong");

        if (!user.PasswordResetConfirmed) throw new BadRequestException("something went wrong");
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        user.PasswordResetConfirmed = false;

        await _userManager.UpdateAsync(user);

        return Result.Success();
    }
}