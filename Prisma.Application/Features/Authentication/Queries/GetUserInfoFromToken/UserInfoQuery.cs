using Ardalis.Result;
using Prisma.Application.Common.DTOs.Auth;
using Prisma.Application.Common.Interfaces;

namespace Prisma.Application.Features.Authentication.Queries.GetUserInfoFromToken;

public sealed record GetUserInfoQuery : ICacheableQuery<Result<LoginCredentials>>
{
    public string BuildCacheKey(ICacheKeyBuilder builder) =>
        builder?.ForUser("user-info") ?? throw new ArgumentNullException(nameof(builder));

    public TimeSpan? Expiration => TimeSpan.FromMinutes(15);
}
