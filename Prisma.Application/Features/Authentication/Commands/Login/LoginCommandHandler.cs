using System.Security.Claims;
using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.DTOs.Auth;

namespace Prisma.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler(
    IIdentityService identityService,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByEmailOrPhoneAsync(request.Email, request.Phone, cancellationToken);

        if (user is null || !await identityService.CheckPasswordAsync(user, request.Password))
        {
            return Result.Error("Invalid Credentials");
        }

        var roles = user.Roles
            .Select(x => x.Role.Name)
            .ToList();

        var permissions = user.Claims
            .Select(c => new Claim(c.ClaimType, c.ClaimValue))
            .ToList();

        var permissionList = permissions.Select(c => c.Value).ToArray();

        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.Email, roles, permissions);

        var refreshToken = jwtTokenService.GenerateRefreshToken();


        //TODO: this must be set from configuration 
        user.UpdateRefreshToken(refreshToken, DateTimeOffset.UtcNow.AddDays(7));

        user.MarkAsOnline();

        await identityService.UpdateAsync(user);

        return new LoginResponse(
            accessToken,
            refreshToken,
            new LoginCredentials(user.Id,
                user.Email,
                user.FirstName,
                user.SecondName,
                Role: roles.FirstOrDefault(),
                permissionList)
        );
    }
}