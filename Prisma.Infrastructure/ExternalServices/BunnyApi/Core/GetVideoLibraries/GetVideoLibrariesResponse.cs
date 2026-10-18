using System.Text.Json.Serialization;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Core.SharedDtos;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Core.GetVideoLibraries;

/// <summary>
/// Paginated response for listing video libraries.
/// </summary>
public sealed record GetVideoLibrariesResponse
{
    [JsonPropertyName("Items")]
    public List<VideoLibraryModel>? Items { get; init; }

    [JsonPropertyName("CurrentPage")]
    public int CurrentPage { get; init; }

    [JsonPropertyName("TotalItems")]
    public int TotalItems { get; init; }

    [JsonPropertyName("HasMoreItems")]
    public bool HasMoreItems { get; init; }
}
