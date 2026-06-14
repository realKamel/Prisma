using System.Security.Claims;

namespace Prisma.Application.Abstractions.Auth;

public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId, string email, IList<string> roles, IList<Claim>? permissions = null);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token);
}