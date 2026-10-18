using System.Text.Json.Serialization;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Core.AddVideoLibrary;

/// <summary>
/// Request payload for adding a new video library.
/// </summary>
public sealed record AddVideoLibraryRequest
{
    /// <summary>
    /// The name of the Video Library. (Required)
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The geo-replication regions of the underlying storage zone.
    /// </summary>
    [JsonPropertyName("ReplicationRegions")]
    public List<string>? ReplicationRegions { get; init; }

    /// <summary>
    /// (Optional) Sets a player version used for this library.
    /// </summary>
    [JsonPropertyName("PlayerVersion")]
    public int? PlayerVersion { get; init; }

    /// <summary>
    /// (Optional) Defines encoding tier. Premium is a paid tier that offers prioritized encoding and extra codec support.
    /// </summary>
    [JsonPropertyName("EncodingTier")]
    public EncodingTier? EncodingTier { get; init; }

    /// <summary>
    /// (Optional) Determines whether JIT encoding should be used for the library. Supported in premium encoding only.
    /// </summary>
    [JsonPropertyName("JitEncodingEnabled")]
    public bool? JitEncodingEnabled { get; init; }

    /// <summary>
    /// (Optional) Specifies which video codecs are used for encoding, provided as a comma-separated (CSV) string.
    /// Free encoding tier supports only x264. A premium encoding tier adds support for vp9, hevc, and av1.
    /// </summary>
    [JsonPropertyName("OutputCodecs")]
    public string? OutputCodecs { get; init; }

    /// <summary>
    /// (Optional) Sets the enabled resolutions for the transcoding. At least one resolution should be enabled.
    /// Possible values: 240p, 360p, 480p, 720p, 1080p, 1440p, 2160p
    /// </summary>
    [JsonPropertyName("EnabledResolutions")]
    public string? EnabledResolutions { get; init; }

    /// <summary>
    /// (Optional) Determines if requests without a referer should be blocked.
    /// </summary>
    [JsonPropertyName("BlockNoneReferrer")]
    public bool? BlockNoneReferrer { get; init; }

    /// <summary>
    /// (Optional) Determines if MP4 fallback should be enabled for this library.
    /// </summary>
    [JsonPropertyName("EnableMP4Fallback")]
    public bool? EnableMP4Fallback { get; init; }

    /// <summary>
    /// (Optional) Determines if the original file should be kept after the video is processed.
    /// </summary>
    [JsonPropertyName("KeepOriginalFiles")]
    public bool? KeepOriginalFiles { get; init; }

    /// <summary>
    /// (Optional) Determines if direct play URLs should be enabled for the library.
    /// </summary>
    [JsonPropertyName("AllowDirectPlay")]
    public bool? AllowDirectPlay { get; init; }

    /// <summary>
    /// (Optional) Determines if multiple output audio track support is enabled on video library.
    /// </summary>
    [JsonPropertyName("EnableMultiAudioTrackSupport")]
    public bool? EnableMultiAudioTrackSupport { get; init; }

    /// <summary>
    /// (Optional) Enables automatic audio transcribing for this library.
    /// </summary>
    [JsonPropertyName("EnableTranscribing")]
    public bool? EnableTranscribing { get; init; }

    /// <summary>
    /// (Optional) Languages that captions will be automatically transcribed to.
    /// </summary>
    [JsonPropertyName("TranscribingCaptionLanguages")]
    public List<string>? TranscribingCaptionLanguages { get; init; }

    /// <summary>
    /// (Optional) Determines if automatic transcribing title generation is enabled for this library.
    /// Enabling any smart generation feature turns on transcribing automatically.
    /// </summary>
    [JsonPropertyName("EnableTranscribingTitleGeneration")]
    public bool? EnableTranscribingTitleGeneration { get; init; }

    /// <summary>
    /// (Optional) Determines if automatic transcribing description generation is enabled for this library.
    /// </summary>
    [JsonPropertyName("EnableTranscribingDescriptionGeneration")]
    public bool? EnableTranscribingDescriptionGeneration { get; init; }

    /// <summary>
    /// (Optional) Determines if automatic transcribing chapters generation is enabled for this library.
    /// </summary>
    [JsonPropertyName("EnableTranscribingChaptersGeneration")]
    public bool? EnableTranscribingChaptersGeneration { get; init; }

    /// <summary>
    /// (Optional) Determines if automatic transcribing moments generation is enabled for this library.
    /// </summary>
    [JsonPropertyName("EnableTranscribingMomentsGeneration")]
    public bool? EnableTranscribingMomentsGeneration { get; init; }

    /// <summary>
    /// (Optional) Enables Early Play. Enabling this also exposes originals via CDN settings consistent with the video library update API.
    /// </summary>
    [JsonPropertyName("AllowEarlyPlay")]
    public bool? AllowEarlyPlay { get; init; }
}

/// <summary>
/// 0 = Free, 1 = Premium
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))] // Remove if you prefer integer serialization
public enum EncodingTier
{
    Free = 0,
    Premium = 1,
}
