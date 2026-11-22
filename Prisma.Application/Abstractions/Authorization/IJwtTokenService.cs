using System.Security.Claims;

namespace Prisma.Application.Abstractions.Authorization;

public interface IJwtTokenService
{
    string GenerateAccessToken(
        Guid userId,
        string email,
        ICollection<string> roles,
        ICollection<Claim>? permissions = default,
        Guid? workspaceId = null
    );

    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token);
}
