using System.Security.Cryptography;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Common.Responses;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities;
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
            return Result.Success();

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        user.PasswordResetCode = code;
        user.PasswordResetCodeExpiry = DateTimeOffset.Now.AddMinutes(10);
        await _userManager.UpdateAsync(user);

        await _emailService.SendAsync(user.Email!, "Reset Password", $"Your code is: {code}");
        return Result.Success();
    }
}
public class ConfirmCodeCommandHandler(
    UserManager<User> _userManager) 
    : IRequestHandler<ConfirmCodeCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ConfirmCodeCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return Result<string>.Failure("Code Invalid");
        if (DateTimeOffset.Now >= user.PasswordResetCodeExpiry) return Result<string>.Failure("Code expired!");
        if (request.Code != user.PasswordResetCode) return Result<string>.Failure("Code Invalid");
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        user.PasswordResetCode = null;
        user.PasswordResetCodeExpiry = null;
        return Result<string>.Success(token);
    }
}
public class ResetPasswordCommandHandler(
    UserManager<User> _userManager) 
    : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return Result<string>.Failure("something went wrong");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            return Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.UpdateAsync(user);
        return Result.Success();
    }
}