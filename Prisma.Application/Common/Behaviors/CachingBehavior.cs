using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Prisma.Application.Common.Interfaces;

namespace Prisma.Application.Common.Behaviors;

public sealed class CachingBehavior<TRequest, TResponse>(HybridCache hybridCache)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        // HybridCache handles stampede protection automatically
        return await hybridCache.GetOrCreateAsync(
            key: request.CacheKey,
            state: next,
            // Factory method to fetch data if missing
            async (nextDelegate, cancel) => await nextDelegate(cancel),
            new HybridCacheEntryOptions
            {
                // Time before the item is removed from both local and distributed cache
                Expiration = request.Expiration ?? TimeSpan.FromMinutes(5),

                // Optional: Time before the item is removed from local memory cache
                // (allows checking distributed cache more frequently)
                LocalCacheExpiration = TimeSpan.FromMinutes(1),
            },
            tags: [request.CacheKey, .. request.CacheTags], // Tags make invalidation much easier!
            cancellationToken: cancellationToken
        );
    }
}
