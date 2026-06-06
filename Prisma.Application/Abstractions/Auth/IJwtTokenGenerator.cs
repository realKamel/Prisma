namespace Prisma.Application.Abstractions.Auth;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string email, IList<string> permissions);
}