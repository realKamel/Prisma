using MediatR;

namespace Prisma.Application.Common.Interfaces;

/// <summary>
/// Represents a query operation that supports caching functionality within the application.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the query.</typeparam>
public interface ICacheableQuery<out TResponse> : IRequest<TResponse>
{
    /// <summary>
    /// The unique cache key for this exact query result.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Optional cache tags for group-based invalidation (e.g., "xzy-list").
    /// </summary>
    IEnumerable<string> CacheTags => []; // Defaults to empty list

    /// <summary>
    /// Optional custom expiration time. If null, the global default is used.
    /// </summary>
    TimeSpan? Expiration { get; }
}
