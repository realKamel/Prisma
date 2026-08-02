using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.Authentication.Commands.EmailVerification;


public class EmailVerificationRequestHandler(
    UserManager<User> _userManager,
    IEmailService _emailService)
    : IRequestHandler<EmailVerificationRequestCommand, Result>
{
    public async Task<Result> Handle(EmailVerificationRequestCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || user.EmailConfirmed)
        {
            return Result.Error("Something Went Wrong");
        }
        var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var verificationLink = $"http://localhost:5117/api/v1/auth/confirm-email?email={request.Email}&token={Uri.EscapeDataString(emailToken)}";
        await _emailService.SendAsync(request.Email, "Email Verification", $"Please verify your email by clicking on the following link: {verificationLink}");
        return Result.SuccessWithMessage("Verification email sent successfully.");
    }
}
public class ConfirmEmailHandler(UserManager<User> _userManager)
    : IRequestHandler<ConfirmEmailCommand, Result>
{
    public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || user.EmailConfirmed)
            return Result.Error("Something Went Wrong");

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);

        if (!result.Succeeded)
            return Result.Error("Invalid token.");

        return Result.SuccessWithMessage("Email Verified successfully.");
    }
}