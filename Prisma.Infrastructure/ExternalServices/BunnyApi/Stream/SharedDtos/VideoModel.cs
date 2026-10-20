using System.Text.Json.Serialization;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.SharedDtos;

public record VideoModel
{
    [JsonPropertyName("videoLibraryId")]
    public long VideoLibraryId { get; init; }

    [JsonPropertyName("guid")]
    public string Guid { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("dateUploaded")]
    public DateTimeOffset DateUploaded { get; init; }

    [JsonPropertyName("views")]
    public long Views { get; init; }

    [JsonPropertyName("isPublic")]
    public bool IsPublic { get; init; }

    [JsonPropertyName("length")]
    public int Length { get; init; }

    [JsonPropertyName("status")]
    public VideoModelStatus Status { get; init; }

    [JsonPropertyName("framerate")]
    public double Framerate { get; init; }

    [JsonPropertyName("rotation")]
    public int? Rotation { get; init; }

    [JsonPropertyName("width")]
    public int Width { get; init; }

    [JsonPropertyName("height")]
    public int Height { get; init; }

    [JsonPropertyName("availableResolutions")]
    public string? AvailableResolutions { get; init; }

    [JsonPropertyName("outputCodecs")]
    public string OutputCodecs { get; init; } = string.Empty;

    [JsonPropertyName("thumbnailCount")]
    public int ThumbnailCount { get; init; }

    [JsonPropertyName("encodeProgress")]
    public int EncodeProgress { get; init; }

    [JsonPropertyName("storageSize")]
    public long StorageSize { get; init; }

    [JsonPropertyName("captions")]
    public List<CaptionModel>? Captions { get; init; }

    [JsonPropertyName("hasMP4Fallback")]
    public bool HasMP4Fallback { get; init; }

    [JsonPropertyName("collectionId")]
    public string? CollectionId { get; init; }

    [JsonPropertyName("thumbnailFileName")]
    public string? ThumbnailFileName { get; init; }

    [JsonPropertyName("thumbnailUrl")]
    public string? ThumbnailUrl { get; init; }

    [JsonPropertyName("thumbnailBlurhash")]
    public string? ThumbnailBlurhash { get; init; }

    [JsonPropertyName("averageWatchTime")]
    public long AverageWatchTime { get; init; }

    [JsonPropertyName("totalWatchTime")]
    public long TotalWatchTime { get; init; }

    [JsonPropertyName("category")]
    public string? Category { get; init; }

    [JsonPropertyName("chapters")]
    public List<ChapterModel>? Chapters { get; init; }

    [JsonPropertyName("moments")]
    public List<MomentModel>? Moments { get; init; }

    [JsonPropertyName("metaTags")]
    public List<MetaTagModel>? MetaTags { get; init; }

    [JsonPropertyName("transcodingMessages")]
    public List<TranscodingMessageModel>? TranscodingMessages { get; init; }

    [JsonPropertyName("jitEncodingEnabled")]
    public bool? JitEncodingEnabled { get; init; }

    [JsonPropertyName("smartGenerateStatus")]
    public SmartGenerateStatus? SmartGenerateStatus { get; init; }

    [JsonPropertyName("smartGenerateFeaturesStatus")]
    public SmartGenerateFeaturesStatusModel? SmartGenerateFeaturesStatus { get; init; }

    [JsonPropertyName("hasOriginal")]
    public bool? HasOriginal { get; init; }

    [JsonPropertyName("originalHash")]
    public string? OriginalHash { get; init; }

    [JsonPropertyName("hasHighQualityPreview")]
    public bool? HasHighQualityPreview { get; init; }
}

public sealed record CaptionModel
{
    [JsonPropertyName("srclang")]
    public string? Srglang { get; init; }

    [JsonPropertyName("label")]
    public string? Label { get; init; }

    [JsonPropertyName("version")]
    public int Version { get; init; }
}

public sealed record ChapterModel
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("start")]
    public int Start { get; init; }

    [JsonPropertyName("end")]
    public int End { get; init; }
}

public sealed record MomentModel
{
    [JsonPropertyName("label")]
    public string Label { get; init; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public int Timestamp { get; init; }
}

public record MetaTagModel
{
    [JsonPropertyName("property")]
    public string? Property { get; init; }

    [JsonPropertyName("value")]
    public string? Value { get; init; }
}

public record TranscodingMessageModel
{
    [JsonPropertyName("timeStamp")]
    public DateTimeOffset TimeStamp { get; init; }

    [JsonPropertyName("level")]
    public Severity Level { get; init; }

    [JsonPropertyName("issueCode")]
    public IssueCodes IssueCode { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("value")]
    public string? Value { get; init; }
}

public sealed record SmartGenerateFeaturesStatusModel
{
    [JsonPropertyName("title")]
    public SmartGenerateStatus? Title { get; init; }

    [JsonPropertyName("description")]
    public SmartGenerateStatus? Description { get; init; }

    [JsonPropertyName("chapters")]
    public SmartGenerateStatus? Chapters { get; init; }

    [JsonPropertyName("moments")]
    public SmartGenerateStatus? Moments { get; init; }
}

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

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SmartGenerateStatus
{
    None = 0,
    Queued = 1,
    InProgress = 2,
    Finished = 3,
    Failed = 4,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Severity
{
    Undefined = 0,
    Information = 1,
    Warning = 2,
    Error = 3,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IssueCodes
{
    Undefined = 0,
    StreamLengthsDifference = 1,
    TranscodingWarnings = 2,
    IncompatibleResolution = 3,
    InvalidFramerate = 4,
    VideoExceededMaxDuration = 5,
    AudioExceededMaxDuration = 6,
    OriginalCorrupted = 7,
    TranscriptionFailed = 8,
    JitIncompatible = 9,
    JitFailed = 10,
    CaptionGenerationFailed = 11,
}
