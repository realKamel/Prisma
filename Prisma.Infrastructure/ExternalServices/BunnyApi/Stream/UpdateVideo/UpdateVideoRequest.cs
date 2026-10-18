using System.Text.Json.Serialization;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.SharedDtos;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.UpdateVideo;

/// <summary>
/// Request payload for updating an existing video.
/// Only non-null properties will be applied.
/// </summary>
public sealed record UpdateVideoRequest
{
    /// <summary>
    /// The title of the video.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// The ID of the collection where the video belongs.
    /// Omit/null to leave unchanged; send an empty string to remove the video from its collection.
    /// </summary>
    [JsonPropertyName("collectionId")]
    public string? CollectionId { get; init; }

    /// <summary>
    /// The list of chapters available for the video (up to 200).
    /// Omit/null to leave unchanged; providing a list (even empty) fully replaces existing chapters.
    /// </summary>
    [JsonPropertyName("chapters")]
    public List<ChapterModel>? Chapters { get; init; }

    /// <summary>
    /// The list of moments available for the video (up to 200).
    /// Omit/null to leave unchanged; providing a list (even empty) fully replaces existing moments.
    /// </summary>
    [JsonPropertyName("moments")]
    public List<MomentModel>? Moments { get; init; }

    /// <summary>
    /// The meta tags added to the video (up to 50).
    /// Omit/null to leave unchanged; providing a list (even empty) fully replaces existing tags.
    /// Setting a tag with Property "description" also updates the video's Description field.
    /// </summary>
    [JsonPropertyName("metaTags")]
    public List<MetaTagModel>? MetaTags { get; init; }
}
