using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Exceptions;

namespace Prisma.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler(
    IIdentityService identityService,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByEmailOrPhoneAsync(request.Email, request.Email);

        if (user is null || !await identityService.CheckPasswordAsync(user, request.Password))
            throw new BadRequestException("Invalid credentials");

        var roles = (await identityService.GetRolesAsync(user)).ToList();

        var permissions = await identityService.GetClaimsAsync(user);

        var accessToken = jwtTokenService
            .GenerateAccessToken(user.Id, user.Email, roles, permissions);

        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;

        user.RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7);

        user.IsOnline = true;

        await identityService.UpdateAsync(user);

        return new LoginResponse(
            accessToken,
            refreshToken,
            new(user.Id,
                user.Email,
                user.FirstName,
                user.SecondName,
                roles.Count == 0 ? null : roles[0]));
    }
}