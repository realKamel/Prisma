using System.Security.Claims;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Application.Features.Authentication.Commands.CleanUserAuthTokens;

public class CleanUserAuthTokensCommandHandler(IJwtTokenService jwtTokenService, IIdentityService identityServices)
    : IRequestHandler<CleanUserAuthTokensCommand>
{
    public async Task Handle(CleanUserAuthTokensCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AccessToken))
        {
            return;
        }

        var principal = jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);

        if (principal is null)
        {
            return;
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
                     principal.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                     principal.FindFirstValue(JwtRegisteredClaimNames.Email);

        if (Guid.TryParse(userId, out Guid userIdAsGuid))
        {
            return;
        }

        var user = await identityServices.FindByIdAsync(userIdAsGuid, cancellationToken);

        if (user is null)
        {
            return;
        }

        // Guard: if the stored refresh token was created AFTER this access token,
        // a newer session owns the user - a stale cleanup job must not wipe it.
        // (Refresh token TTL is 7 days, set in Login/Refresh handlers.)
        if (user.RefreshTokenExpiry is { } refreshExpiry &&
            long.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Iat),
                out var iatUnix))
        {
            var accessTokenIssuedAt = DateTimeOffset.FromUnixTimeSeconds(iatUnix);
            var refreshTokenCreatedAt = refreshExpiry.AddDays(-7);
            if (refreshTokenCreatedAt > accessTokenIssuedAt)
            {
                return;
            }
        }

        user.ClearRefreshTokens();
        user.UpdateOnlineStatus(false);

        await identityServices.UpdateAsync(user);
    }
}