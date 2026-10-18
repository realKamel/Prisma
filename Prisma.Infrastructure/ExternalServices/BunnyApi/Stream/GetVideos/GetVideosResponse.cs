using System.Text.Json.Serialization;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.SharedDtos;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.GetVideos;

/// <summary>
/// Paginated response model for listing videos.
/// </summary>
public sealed record GetVideosResponse
{
    /// <summary>
    /// The total number of items across all pages matching the query, not just the current page.
    /// </summary>
    [JsonPropertyName("totalItems")]
    public long TotalItems { get; init; }

    /// <summary>
    /// The current page number of the result set (1-based).
    /// </summary>
    [JsonPropertyName("currentPage")]
    public long CurrentPage { get; init; }

    /// <summary>
    /// The effective page size after clamping the requested value; the last page may contain fewer items.
    /// </summary>
    [JsonPropertyName("itemsPerPage")]
    public int ItemsPerPage { get; init; }

    /// <summary>
    /// The items on the current page, in the order returned by the query; an empty list if no results match.
    /// </summary>
    [JsonPropertyName("items")]
    public List<VideoModel>? Items { get; init; }
}
