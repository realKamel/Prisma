using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.Authentication.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    UserManager<User> userManager,
    IJwtTokenService jwtService) : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate the expired access token and extract claims
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result.Unauthorized("Please Login");
        }

        var principal = jwtService.GetPrincipalFromExpiredToken(request.AccessToken);

        if (principal is null)
        {
            return Result.Error("Please Login");
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Unauthorized("Please Login");
        }

        var user = await userManager.FindByIdAsync(userId);

        if (user is null || user.RefreshToken != request.RefreshToken ||
            user.RefreshTokenExpiry < DateTimeOffset.UtcNow)
        {
            return Result.Unauthorized("Please Login");
        }

        var claims = await userManager.GetClaimsAsync(user);
        var roles = await userManager.GetRolesAsync(user);

        var newAccessToken = jwtService.GenerateAccessToken(user.Id, user.Email, roles, claims);

        var newRefreshToken = jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7);

        await userManager.UpdateAsync(user);

        return new AuthResponse(newAccessToken, newRefreshToken);
    }
}