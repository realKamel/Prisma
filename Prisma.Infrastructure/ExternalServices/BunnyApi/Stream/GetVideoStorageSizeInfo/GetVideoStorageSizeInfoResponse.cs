using System.Text.Json.Serialization;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.GetVideoStorageSizeInfo;

/// <summary>
/// Specialized status response that includes video storage size data.
/// Inherits base properties (Success, Message, StatusCode) from StatusModel.
/// </summary>
public sealed record GetVideoStorageSizeInfoResponse : BunnyStreamGeneralResponse
{
    /// <summary>
    /// The response payload containing the storage size breakdown.
    /// </summary>
    [JsonPropertyName("data")]
    public VideoStorageSizeModel? Data { get; init; }
}

/// <summary>
/// Detailed breakdown of storage used by a specific video.
/// </summary>
public sealed record VideoStorageSizeModel
{
    /// <summary>
    /// Size in bytes of each encoded rendition, keyed by "{resolution}-{codec}"
    /// (e.g., "720p-h264" or legacy "1920x1080-h264").
    /// </summary>
    [JsonPropertyName("encoded")]
    public Dictionary<string, CodecRenditionSizeModel>? Encoded { get; init; }

    /// <summary>
    /// Size in bytes of the video's JPEG thumbnails.
    /// </summary>
    [JsonPropertyName("thumbnails")]
    public long Thumbnails { get; init; }

    /// <summary>
    /// Size in bytes of the video's GIF and WEBP previews.
    /// </summary>
    [JsonPropertyName("previews")]
    public long Previews { get; init; }

    /// <summary>
    /// Size in bytes of the original uploaded video file.
    /// </summary>
    [JsonPropertyName("originals")]
    public long Originals { get; init; }

    /// <summary>
    /// Size in bytes of the MP4 fallback rendition.
    /// </summary>
    [JsonPropertyName("mp4Fallback")]
    public long Mp4Fallback { get; init; }

    /// <summary>
    /// Size in bytes of everything else: playlists, manifests, captions, audio extracts, etc.
    /// </summary>
    [JsonPropertyName("miscellaneous")]
    public long Miscellaneous { get; init; }

    /// <summary>
    /// Timestamp when this storage size was calculated (UTC).
    /// </summary>
    [JsonPropertyName("calculatedAt")]
    public DateTimeOffset CalculatedAt { get; init; }
}

/// <summary>
/// Represents the storage size of a specific encoded video rendition.
/// </summary>
public sealed record CodecRenditionSizeModel
{
    /// <summary>
    /// Output codec name as used by the storage layout (e.g., "h264", "hevc", "vp9", "av1").
    /// Note: h264 is reported as "x264" in VideoModel.OutputCodecs.
    /// </summary>
    [JsonPropertyName("codec")]
    public string? Codec { get; init; }

    /// <summary>
    /// Resolution of this rendition (e.g., "720p", "1080p"), or a legacy "{width}x{height}" value.
    /// </summary>
    [JsonPropertyName("resolution")]
    public string? Resolution { get; init; }

    /// <summary>
    /// Size of this rendition in bytes.
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; init; }
}
