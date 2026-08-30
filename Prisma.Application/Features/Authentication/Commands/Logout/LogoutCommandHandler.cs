using System.Security.Claims;
using Ardalis.Result;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Application.Features.Authentication.Commands.Logout;

public class LogoutCommandHandler(
    IJwtTokenService jwtTokenService,
    IIdentityService identityService,
    ITokenService tokenService
) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AccessToken))
        {
            return Result.NoContent();
        }

        var principal = jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);

        if (principal is null)
        {
            return Result.NoContent();
        }

        var userId =
            principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (Guid.TryParse(userId, out Guid userIdAsGuid))
        {
            return Result.NoContent();
        }

        var user = await identityService.FindByIdAsync(userIdAsGuid, cancellationToken);

        if (user is null)
        {
            return Result.NoContent();
        }

        await tokenService.RevokeRefreshTokenAsync(user.Id);

        return Result.NoContent();
    }
}
