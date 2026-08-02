using System.Security.Claims;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.UserAggregate;


namespace Prisma.Application.Features.Authentication.Commands.Logout;

public class LogoutCommandHandler(IJwtTokenService jwtTokenService, UserManager<User> userManager)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AccessToken))
        {
            return Result.Invalid();
        }

        var principal = jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);

        if (principal is null)
        {
            return Result.Invalid();
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
                     principal.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                     principal.FindFirstValue(JwtRegisteredClaimNames.Email);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result.Unauthorized();
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        user.IsOnline = false;

        await userManager.UpdateAsync(user);

        return Result.Success();
    }
}