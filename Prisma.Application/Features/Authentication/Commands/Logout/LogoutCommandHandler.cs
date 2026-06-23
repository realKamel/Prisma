using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.UserAggregate;


namespace Prisma.Application.Features.Authentication.Commands.Logout;

public class LogoutCommandHandler(IJwtTokenService jwtTokenService, UserManager<User> userManager)
    : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
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

        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var user = await userManager.FindByEmailAsync(userId);

        if (user is null)
        {
            return;
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        user.IsOnline = false;

        await userManager.UpdateAsync(user);
    }
}