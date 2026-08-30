using System.Security.Claims;

namespace Prisma.Application.Abstractions.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(
        Guid userId,
        string email,
        ICollection<string> roles,
        ICollection<Claim>? permissions = default
    );
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token);
}
