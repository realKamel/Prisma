using System.Security.Claims;
using Ardalis.Result;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Application.Features.Authentication.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    IJwtTokenService jwtService
) : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken
    )
    {
        // 1. Validate the expired access token and extract claims
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result.Unauthorized("COMMON.UNAUTHORIZED");
        }

        var principal = jwtService.GetPrincipalFromExpiredToken(request.AccessToken);

        if (principal is null)
        {
            return Result.Unauthorized("COMMON.UNAUTHORIZED");
        }

        var userId =
            principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Unauthorized("COMMON.UNAUTHORIZED");
        }

        if (!Guid.TryParse(userId, out Guid id))
        {
            return Result.Unauthorized("COMMON.UNAUTHORIZED");
        }

        var user = await identityService.FindByIdAsync(id, false, cancellationToken);

        if (user is null)
        {
            return Result.Unauthorized("COMMON.UNAUTHORIZED");
        }

        var permissions = user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();

        var roles = user.Roles.Select(x => x.Role.Name).ToList();

        var newAccessToken = jwtService.GenerateAccessToken(
            user.Id,
            user.Email,
            roles,
            permissions
        );

        var newRefreshToken = jwtService.GenerateRefreshToken();

        await tokenService.SaveRefreshTokenAsync(user.Id, newRefreshToken);

        return new AuthResponse(newAccessToken, newRefreshToken);
    }
}
