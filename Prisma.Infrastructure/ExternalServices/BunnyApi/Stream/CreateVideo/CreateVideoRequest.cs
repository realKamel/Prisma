using System.Text.Json.Serialization;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.CreateVideo;

public sealed record CreateVideoRequest
{
    /// <summary>
    /// The title of the video. (Required)
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the collection where the video will be put.
    /// </summary>
    [JsonPropertyName("collectionId")]
    public string? CollectionId { get; init; }

    /// <summary>
    /// Video time in milliseconds to extract the main video thumbnail.
    /// </summary>
    [JsonPropertyName("thumbnailTime")]
    public int? ThumbnailTime { get; init; }
}
