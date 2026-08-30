using Microsoft.Extensions.Options;
using Prisma.Application.Abstractions.Services;
using Prisma.Infrastructure.Services.Auth;
using StackExchange.Redis;

namespace Prisma.Infrastructure.Caching;

internal sealed class ValkeyTokenService(
    IConnectionMultiplexer valkey,
    IOptions<JwtSettingsOptions> jwtOptions
) : ITokenService
{
    private readonly IDatabase _db = valkey.GetDatabase();

    // 1. Store Refresh Token with automatic TTL matching its lifetime (e.g., 7 days)
    public async Task SaveRefreshTokenAsync(
        Guid userId,
        string refreshToken,
        TimeSpan? expiry = default
    )
    {
        string key = $"refreshToken:{userId}";

        expiry ??= TimeSpan.FromDays(jwtOptions.Value.RefreshTokenExpiryInDays);

        await _db.StringSetAsync(key, refreshToken, expiry.Value);
    }

    // 2. Validate Refresh Token
    public async Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
    {
        string key = $"refreshToken:{userId}";

        var storedToken = await _db.StringGetAsync(key);

        return storedToken.HasValue && storedToken.ToString() == refreshToken;
    }

    // 3. Delete Refresh Token on Logout
    public async Task RevokeRefreshTokenAsync(Guid userId)
    {
        string key = $"refreshToken:{userId}";
        await _db.KeyDeleteAsync(key);
    }

    // 4. Blacklist compromised/logged-out Access Token by its JTI (Jwt ID)
    public async Task BlacklistAccessTokenAsync(string jti, TimeSpan remainingTime)
    {
        string key = $"blacklist:{jti}";
        await _db.StringSetAsync(key, "revoked", remainingTime);
    }

    // 5. Check if Access Token is blacklisted
    public async Task<bool> IsAccessTokenBlacklistedAsync(string jti)
    {
        string key = $"blacklist:{jti}";
        return await _db.KeyExistsAsync(key);
    }
}
