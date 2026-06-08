using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Prisma.Application.Abstractions.Auth;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities;

namespace Prisma.Application.Features.Authentication.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    UserManager<User> userManager,
    IJwtTokenService jwtService) : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate the expired access token and extract claims
        var principal = jwtService.GetPrincipalFromExpiredToken(request.AccessToken);

        if (principal is null)
            return Result<AuthResponse>.Failure("Invalid access token.");

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
                     principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        var user = await userManager.FindByIdAsync(userId!);

        if (user is null
            || user.RefreshToken != request.RefreshToken
            || user.RefreshTokenExpiry < DateTimeOffset.UtcNow)
            return Result<AuthResponse>.Failure("Invalid or expired refresh token.");

        var claims = await userManager.GetClaimsAsync(user);
        var userClaims = claims.Select(claim => claim.Value).ToList();

        var newAccessToken = jwtService.GenerateAccessToken(user.Id, user.Email, userClaims);
        var newRefreshToken = jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7);

        await userManager.UpdateAsync(user);

        return Result<AuthResponse>.Success(new AuthResponse(
            newAccessToken,
            newRefreshToken,
            DateTime.UtcNow.AddMinutes(15)));
    }
}