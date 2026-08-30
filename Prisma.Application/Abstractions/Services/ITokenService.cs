namespace Prisma.Application.Abstractions.Services;

public interface ITokenService
{
    Task SaveRefreshTokenAsync(Guid userId, string refreshToken, TimeSpan? expiry = default);
    Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken);
    Task RevokeRefreshTokenAsync(Guid userId);
    Task BlacklistAccessTokenAsync(string jti, TimeSpan remainingTime);
    Task<bool> IsAccessTokenBlacklistedAsync(string jti);
}
