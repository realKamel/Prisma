using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Auth;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities;

namespace Prisma.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler(
    UserManager<User> userManager,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Result<LoginResponse>.Failure("Invalid credentials.");

        var claims = await userManager.GetClaimsAsync(user);

        //TODO: To Be changed
        var permissions = claims.Select(x => x.Type).ToList();

        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.Email, permissions);

        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;

        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await userManager.UpdateAsync(user);

        return Result<LoginResponse>.Success(new LoginResponse(accessToken, refreshToken));
    }
}