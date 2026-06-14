using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Prisma.Application.Abstractions.Auth;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;

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
            throw new UnauthorizedException("Please Login");
        }

        var principal = jwtService.GetPrincipalFromExpiredToken(request.AccessToken);

        if (principal is null)
        {
            throw new BadRequestException("Please Login");
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
                     principal.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                     principal.FindFirstValue(JwtRegisteredClaimNames.Email);

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedException("Please Login");
        }

        var user = await userManager.FindByEmailAsync(userId);

        if (user is null || user.RefreshToken != request.RefreshToken ||
            user.RefreshTokenExpiry < DateTimeOffset.UtcNow)
        {
            throw new UnauthorizedException("Please Login");
        }

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