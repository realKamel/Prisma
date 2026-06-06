namespace Prisma.Application.Abstractions.Auth;

public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId, string email, IList<string> permissions);
    string GenerateRefreshToken();
}