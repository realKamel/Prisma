using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Auth;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.Authentication.Commands.Register;

public class RegisterCommandHandler(
    UserManager<User> userManager,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            return Result<RegisterResponse>.Failure(existingUser.Email);
        }

        var user = new Student
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            SecondName = request.SecondName,
            ThirdName = request.ThirdName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return Result<RegisterResponse>.Failure(result.Errors.First().Description);

        await userManager.AddClaimsAsync(user, new Claim[]
        {
            new Claim(AppClaims.PermissionsClaim,
                AppClaims.Permissions.SubmitAssignment)
        });

        var accessToken =
            jwtTokenService.GenerateAccessToken(user.Id, user.Email, [AppClaims.Permissions.ManageStudents]);

        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;

        user.RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7);

        await userManager.UpdateAsync(user);

        return Result<RegisterResponse>.Success(new RegisterResponse(accessToken, refreshToken));
    }
}