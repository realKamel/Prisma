using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.DTOs.Auth;

namespace Prisma.Application.Features.Authentication.Queries.GetUserInfoFromToken;

public class GetUserInfoQueryHandler(
    ICurrentUserService currentUserService,
    IIdentityService identityService,
    HybridCache cache
) : IRequestHandler<GetUserInfoQuery, Result<LoginCredentials>>
{
    public async Task<Result<LoginCredentials>> Handle(
        GetUserInfoQuery request,
        CancellationToken cancellationToken
    )
    {
        if (!currentUserService.IsAuthenticated || currentUserService.Email is null)
        {
            return Result.Unauthorized("COMMON.UNAUTHORIZED");
        }

        string userEmail = currentUserService.Email;

        string cacheKey = $"user-info:{userEmail}";

        // HybridCache handles L1 memory cache, L2 Valkey cache, and DB Stampede Protection
        var credentials = await cache.GetOrCreateAsync(
            cacheKey,
            async token => await FetchUserInfoFromDatabaseAsync(userEmail),
            options: new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(15), // Max lifetime in cache
                LocalCacheExpiration = TimeSpan.FromMinutes(5), // In-memory L1 cache duration
            },
            cancellationToken: cancellationToken
        );

        if (credentials is null)
        {
            return Result.Unauthorized("COMMON.UNAUTHORIZED");
        }

        return credentials;
    }

    private async Task<LoginCredentials?> FetchUserInfoFromDatabaseAsync(string email)
    {
        var user = await identityService.FindByEmailAsync(email);

        if (user is null)
        {
            return null;
        }

        var roles = await identityService.GetRolesAsync(user);
        var permissions = (await identityService.GetClaimsAsync(user))
            .Select(claim => claim.Value)
            .ToArray();

        return new LoginCredentials(
            user.Id,
            user.Email,
            user.FirstName,
            user.SecondName,
            roles.FirstOrDefault(),
            permissions
        );
    }
}
