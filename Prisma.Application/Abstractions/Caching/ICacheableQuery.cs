using Prisma.Application.Abstractions.Messaging;

namespace Prisma.Application.Abstractions.Caching;

/// <summary>
/// Represents a query operation that supports caching functionality within the application.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the query.</typeparam>
public interface ICacheableQuery<out TResponse> : IQuery<TResponse>
{
    /// <summary>
    /// Builds the unique cache key for this query.
    /// </summary>
    string BuildCacheKey(ICacheKeyBuilder builder);

    /// <summary>
    /// Optional cache tags for group-based invalidation.
    /// </summary>
    IEnumerable<string> CacheTags => [];

    /// <summary>
    /// Optional custom expiration time.
    /// </summary>
    TimeSpan? Expiration => null;
}
