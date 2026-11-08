using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Prisma.Application.Abstractions.Caching;

namespace Prisma.Application.Common.Behaviors;

public sealed class CachingBehavior<TRequest, TResponse>(
    HybridCache hybridCache,
    ICacheKeyBuilder builder
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var key = request.BuildCacheKey(builder);

        return await hybridCache.GetOrCreateAsync(
            key: key,
            state: next,
            async (nextDelegate, cancel) => await nextDelegate(cancel),
            new HybridCacheEntryOptions
            {
                Expiration = request.Expiration ?? TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(1),
            },
            tags: [.. request.CacheTags],
            cancellationToken: cancellationToken
        );
    }
}
