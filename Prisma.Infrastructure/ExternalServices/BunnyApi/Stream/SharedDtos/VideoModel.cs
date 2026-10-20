using System.Text.Json.Serialization;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.SharedDtos;

/// <summary>
/// Represents a video entity in the bunny.net Stream API.
/// </summary>
public record VideoModel
{
    /// <summary>
    /// The ID of the video library that the video belongs to.
    /// </summary>
    [JsonPropertyName("videoLibraryId")]
    public long VideoLibraryId { get; init; }

    /// <summary>
    /// The unique ID of the video.
    /// </summary>
    [JsonPropertyName("guid")]
    public string Guid { get; init; } = string.Empty;

    /// <summary>
    /// The title of the video.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// The description of the video.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// The date when the video was uploaded.
    /// </summary>
    [JsonPropertyName("dateUploaded")]
    public DateTimeOffset DateUploaded { get; init; }

    /// <summary>
    /// The number of views the video received.
    /// </summary>
    [JsonPropertyName("views")]
    public long Views { get; init; }

    /// <summary>
    /// Determines if the video is publicly accessible.
    /// </summary>
    [JsonPropertyName("isPublic")]
    public bool IsPublic { get; init; }

    /// <summary>
    /// The duration of the video in seconds.
    /// </summary>
    [JsonPropertyName("length")]
    public int Length { get; init; }

    /// <summary>
    /// The status of the video. See <see cref="VideoModelStatus"/>.
    /// </summary>
    [JsonPropertyName("status")]
    public VideoModelStatus Status { get; init; }

    /// <summary>
    /// The framerate of the video.
    /// </summary>
    [JsonPropertyName("framerate")]
    public double Framerate { get; init; }

    /// <summary>
    /// Rotation of the source file in degrees as read from container metadata (e.g. 90, -90, 180, 270); null when no rotation metadata is present.
    /// </summary>
    [JsonPropertyName("rotation")]
    public int? Rotation { get; init; }

    /// <summary>
    /// The width of the original video file.
    /// </summary>
    [JsonPropertyName("width")]
    public int Width { get; init; }

    /// <summary>
    /// The height of the original video file.
    /// </summary>
    [JsonPropertyName("height")]
    public int Height { get; init; }

    /// <summary>
    /// Comma-separated list of resolution labels (e.g. "360p,720p,1080p") that have finished encoding and are available for playback.
    /// </summary>
    [JsonPropertyName("availableResolutions")]
    public string? AvailableResolutions { get; init; }

    /// <summary>
    /// Comma-separated list of output codecs generated for this video (e.g. "x264,vp9"). See EncoderOutputCodec for possible values.
    /// </summary>
    [JsonPropertyName("outputCodecs")]
    public string OutputCodecs { get; init; } = string.Empty;

    /// <summary>
    /// The number of thumbnails generated for this video.
    /// </summary>
    [JsonPropertyName("thumbnailCount")]
    public int ThumbnailCount { get; init; }

    /// <summary>
    /// Encoding progress as a percentage from 0 to 100; reaches 100 when transcoding finishes.
    /// </summary>
    [JsonPropertyName("encodeProgress")]
    public int EncodeProgress { get; init; }

    /// <summary>
    /// Total storage used by this video, in bytes.
    /// </summary>
    [JsonPropertyName("storageSize")]
    public long StorageSize { get; init; }

    /// <summary>
    /// The list of captions available for the video.
    /// </summary>
    [JsonPropertyName("captions")]
    public List<CaptionModel>? Captions { get; init; }

    /// <summary>
    /// Determines if the video has MP4 fallback files generated.
    /// </summary>
    [JsonPropertyName("hasMP4Fallback")]
    public bool HasMP4Fallback { get; init; }

    /// <summary>
    /// The ID of the collection where the video belongs.
    /// </summary>
    [JsonPropertyName("collectionId")]
    public string? CollectionId { get; init; }

    /// <summary>
    /// The file name of the thumbnail inside of the storage.
    /// </summary>
    [JsonPropertyName("thumbnailFileName")]
    public string? ThumbnailFileName { get; init; }

    /// <summary>
    /// The CDN URL of the thumbnail.
    /// </summary>
    [JsonPropertyName("thumbnailUrl")]
    public string? ThumbnailUrl { get; init; }

    /// <summary>
    /// Thumbnail blurhash to show while the actual thumbnail is being loaded.
    /// </summary>
    [JsonPropertyName("thumbnailBlurhash")]
    public string? ThumbnailBlurhash { get; init; }

    /// <summary>
    /// The average watch time of the video in seconds.
    /// </summary>
    [JsonPropertyName("averageWatchTime")]
    public long AverageWatchTime { get; init; }

    /// <summary>
    /// The total video watch time in seconds.
    /// </summary>
    [JsonPropertyName("totalWatchTime")]
    public long TotalWatchTime { get; init; }

    /// <summary>
    /// Automatically detected content category label; "untagged" when no category has been determined yet.
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; init; }

    /// <summary>
    /// The list of chapters available for the video.
    /// </summary>
    [JsonPropertyName("chapters")]
    public List<ChapterModel>? Chapters { get; init; }

    /// <summary>
    /// The list of moments available for the video.
    /// </summary>
    [JsonPropertyName("moments")]
    public List<MomentModel>? Moments { get; init; }

    /// <summary>
    /// The list of meta tags that have been added to the video.
    /// </summary>
    [JsonPropertyName("metaTags")]
    public List<MetaTagModel>? MetaTags { get; init; }

    /// <summary>
    /// The list of transcoding messages that describe potential issues while the video was transcoding.
    /// </summary>
    [JsonPropertyName("transcodingMessages")]
    public List<TranscodingMessageModel>? TranscodingMessages { get; init; }

    /// <summary>
    /// Whether just-in-time encoding is enabled for this specific video, overriding the video library's default JitEncodingEnabled setting when set. Null means the library default applies.
    /// </summary>
    [JsonPropertyName("jitEncodingEnabled")]
    public bool? JitEncodingEnabled { get; init; }

    /// <summary>
    /// Aggregate smart generate status derived from per-feature statuses when present: InProgress, then Queued, then Failed, then Finished; otherwise the stored legacy value. See SmartGenerateFeaturesStatus.
    /// </summary>
    [JsonPropertyName("smartGenerateStatus")]
    public SmartGenerateStatus? SmartGenerateStatus { get; init; }

    /// <summary>
    /// Per-feature smart generate statuses (title, description, chapters, moments). Null for a feature on legacy rows.
    /// </summary>
    [JsonPropertyName("smartGenerateFeaturesStatus")]
    public SmartGenerateFeaturesStatusModel? SmartGenerateFeaturesStatus { get; init; }

    /// <summary>
    /// Determines if video has the original file available in storage.
    /// </summary>
    [JsonPropertyName("hasOriginal")]
    public bool? HasOriginal { get; init; }

    /// <summary>
    /// Original uploaded file SHA256 hash.
    /// </summary>
    [JsonPropertyName("originalHash")]
    public string? OriginalHash { get; init; }

    /// <summary>
    /// Whether a high-quality animated preview has been generated for this video.
    /// </summary>
    [JsonPropertyName("hasHighQualityPreview")]
    public bool? HasHighQualityPreview { get; init; }
}

/// <summary>
/// Represents a caption associated with a video.
/// </summary>
public sealed record CaptionModel
{
    /// <summary>
    /// The unique srclang shortcode for the caption.
    /// </summary>
    [JsonPropertyName("srclang")]
    public string? Srglang { get; init; }

    /// <summary>
    /// The text description label for the caption.
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; init; }

    /// <summary>
    /// Increments each time this caption is replaced; useful as a cache-busting value for the caption file URL.
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; init; }
}

/// <summary>
/// Represents a timestamp-based chapter within a video.
/// </summary>
public sealed record ChapterModel
{
    /// <summary>
    /// The title of the chapter, up to 255 characters.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// The start time of the chapter in seconds. Must be less than End and within the video's duration; chapters must not overlap each other.
    /// </summary>
    [JsonPropertyName("start")]
    public int Start { get; init; }

    /// <summary>
    /// The end time of the chapter in seconds. Must be greater than Start and within the video's duration; chapters must not overlap each other.
    /// </summary>
    [JsonPropertyName("end")]
    public int End { get; init; }
}

/// <summary>
/// Represents a specific highlighted moment within a video.
/// </summary>
public sealed record MomentModel
{
    /// <summary>
    /// The text description label for the moment, up to 255 characters.
    /// </summary>
    [JsonPropertyName("label")]
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// The timestamp of the moment in seconds. Must be within the video's duration.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public int Timestamp { get; init; }
}

/// <summary>
/// Represents a key-value meta tag attached to a video.
/// </summary>
public record MetaTagModel
{
    /// <summary>
    /// The meta tag key. Must match ^[A-Za-z0-9._:'"-]+$. Setting a tag with the property "description" also updates the video's Description field.
    /// </summary>
    [JsonPropertyName("property")]
    public string? Property { get; init; }

    /// <summary>
    /// The meta tag value. HTML/markup is stripped and the value is stored as plain text.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; init; }
}

/// <summary>
/// Represents a notification or issue message generated during the video transcoding process.
/// </summary>
public record TranscodingMessageModel
{
    /// <summary>
    /// Date and time when this message was added (UTC).
    /// </summary>
    [JsonPropertyName("timeStamp")]
    public DateTimeOffset TimeStamp { get; init; }

    /// <summary>
    /// Message severity. See <see cref="Severity"/>.
    /// </summary>
    [JsonPropertyName("level")]
    public Severity Level { get; init; }

    /// <summary>
    /// Issue code identifying the specific transcoding problem. See <see cref="IssueCodes"/>.
    /// </summary>
    [JsonPropertyName("issueCode")]
    public IssueCodes IssueCode { get; init; }

    /// <summary>
    /// Human-readable description of the issue.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    /// <summary>
    /// Additional context value for the issue, if applicable; otherwise empty.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; init; }
}

/// <summary>
/// Represents the per-feature status of AI Smart Generation tasks.
/// </summary>
public sealed record SmartGenerateFeaturesStatusModel
{
    /// <summary>
    /// Smart generate status for the video title, when applicable.
    /// </summary>
    [JsonPropertyName("title")]
    public SmartGenerateStatus? Title { get; init; }

    /// <summary>
    /// Smart generate status for the video description, when applicable.
    /// </summary>
    [JsonPropertyName("description")]
    public SmartGenerateStatus? Description { get; init; }

    /// <summary>
    /// Smart generate status for chapters, when applicable.
    /// </summary>
    [JsonPropertyName("chapters")]
    public SmartGenerateStatus? Chapters { get; init; }

    /// <summary>
    /// Smart generate status for moments, when applicable.
    /// </summary>
    [JsonPropertyName("moments")]
    public SmartGenerateStatus? Moments { get; init; }
}

/// <summary>
/// Defines the processing states of a video.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VideoModelStatus
{
    Created = 0,
    Uploaded = 1,
    Processing = 2,
    Transcoding = 3,
    Finished = 4,
    Error = 5,
    UploadFailed = 6,
    JitSegmenting = 7,
    JitPlaylistsCreated = 8,
}

/// <summary>
/// Defines the status of an AI smart generation operation.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SmartGenerateStatus
{
    None = 0,
    Queued = 1,
    InProgress = 2,
    Finished = 3,
    Failed = 4,
}

/// <summary>
/// Defines the severity level of a transcoding message.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Severity
{
    /// <summary>
    /// Shouldn't be used.
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// An info message to the user, shouldn't break anything.
    /// </summary>
    Information = 1,

    /// <summary>
    /// A warning message, for example that the transcoded video may break when playing.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// An error, meaning that the video will most likely not be playable.
    /// </summary>
    Error = 3,
}

/// <summary>
/// Defines issue codes identifying specific transcoding problems.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IssueCodes
{
    /// <summary>
    /// Shouldn't be used.
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// Marks that there is a mismatch between lengths of audio and video streams in the transcoded file.
    /// </summary>
    StreamLengthsDifference = 1,

    /// <summary>
    /// Marks that transcoding completed with warnings.
    /// </summary>
    TranscodingWarnings = 2,

    /// <summary>
    /// Marks that an original video file has incompatible resolution or isn't a video stream.
    /// </summary>
    IncompatibleResolution = 3,

    /// <summary>
    /// Marks that an original video file has invalid framerate.
    /// </summary>
    InvalidFramerate = 4,

    /// <summary>
    /// Marks that video stream exceeded max duration.
    /// </summary>
    VideoExceededMaxDuration = 5,

    /// <summary>
    /// Marks that audio stream exceeded max duration.
    /// </summary>
    AudioExceededMaxDuration = 6,

    /// <summary>
    /// Marks that a video file is corrupted and couldn't be processed properly.
    /// </summary>
    OriginalCorrupted = 7,

    /// <summary>
    /// Marks that video transcription finished with errors and might not be complete.
    /// </summary>
    TranscriptionFailed = 8,

    /// <summary>
    /// Marks that a video file is not compatible with JIT encoding.
    /// </summary>
    JitIncompatible = 9,

    /// <summary>
    /// Marks that video failed JIT encoding.
    /// </summary>
    JitFailed = 10,

    /// <summary>
    /// Marks that captions generation failed.
    /// </summary>
    CaptionGenerationFailed = 11,
}
