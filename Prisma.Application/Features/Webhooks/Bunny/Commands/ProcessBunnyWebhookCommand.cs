using System.Text.Json.Serialization;
using Ardalis.Result;
using Prisma.Application.Abstractions.Messaging;

namespace Prisma.Application.Features.Webhooks.Bunny.Commands;

public sealed record ProcessBunnyWebhookCommand : ICommand<Result>
{
    public long VideoLibraryId { get; init; }

    public Guid VideoGuid { get; init; }

    public VideoWebhookStatus Status { get; init; }

    public Guid EventId { get; init; }

    public DateTimeOffset Timestamp { get; init; }
    public string? Reason { get; init; }
};

/// <summary>
/// Represents a video webhook callback payload.
/// </summary>
public sealed record VideoWebhookPayload
{
    /// <summary>
    /// Indicates whether this is a live stream webhook (<c>false</c> for video webhooks).
    /// </summary>
    [JsonPropertyName("IsLiveStreamWebhook")]
    public bool IsLiveStreamWebhook { get; init; }

    /// <summary>
    /// The ID of the video library.
    /// </summary>
    [JsonPropertyName("VideoLibraryId")]
    public long VideoLibraryId { get; init; }

    /// <summary>
    /// The ID (GUID) of the video.
    /// </summary>
    [JsonPropertyName("VideoGuid")]
    public Guid VideoGuid { get; init; }

    /// <summary>
    /// The numeric status code of the event. See <see cref="VideoWebhookStatus"/>.
    /// </summary>
    [JsonPropertyName("Status")]
    public VideoWebhookStatus Status { get; init; }

    /// <summary>
    /// A unique ID for the event. Stays the same across duplicate deliveries; use it to deduplicate events.
    /// </summary>
    [JsonPropertyName("EventId")]
    public Guid EventId { get; init; }

    /// <summary>
    /// The UTC timestamp when the event occurred (not delivery time).
    /// Deliveries can arrive out of order; use this to order events per video.
    /// </summary>
    [JsonPropertyName("Timestamp")]
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Extra detail for statuses that provide one (such as <see cref="VideoWebhookStatus.SmartGenerationFailed"/>,
    /// where it names the failed feature: Title, Description, Chapters, or Moments).
    /// Omitted when not applicable.
    /// </summary>
    [JsonPropertyName("Reason")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Reason { get; init; }
}

/// <summary>
/// Numeric status codes sent with a <see cref="VideoWebhookDto"/> payload.
/// </summary>
public enum VideoWebhookStatus
{
    /// <summary>The video has been queued for encoding.</summary>
    Queued = 0,

    /// <summary>The video has begun processing the preview and format details.</summary>
    Processing = 1,

    /// <summary>The video is encoding.</summary>
    Encoding = 2,

    /// <summary>The video encoding has finished and the video is fully available.</summary>
    Finished = 3,

    /// <summary>
    /// The encoder has finished processing one of the resolutions.
    /// The first delivery also signals that the video is now playable.
    /// </summary>
    ResolutionFinished = 4,

    /// <summary>The video encoding failed. The video has finished processing.</summary>
    Failed = 5,

    /// <summary>A pre-signed upload has been initiated.</summary>
    PresignedUploadStarted = 6,

    /// <summary>A pre-signed upload has been completed.</summary>
    PresignedUploadFinished = 7,

    /// <summary>A pre-signed upload has failed.</summary>
    PresignedUploadFailed = 8,

    /// <summary>Automatic captions were generated.</summary>
    CaptionsGenerated = 9,

    /// <summary>Automatic generation of title or description has been completed.</summary>
    TitleOrDescriptionGenerated = 10,

    /// <summary>Automatic caption generation has failed.</summary>
    CaptionGenerationFailed = 11,

    /// <summary>Automatic chapters were generated.</summary>
    ChaptersGenerated = 12,

    /// <summary>Automatic moments were generated.</summary>
    MomentsGenerated = 13,

    /// <summary>
    /// The video was deleted, including when deleted together with its collection.
    /// Sent once per deleted video.
    /// </summary>
    VideoDeleted = 32,

    /// <summary>
    /// A requested smart generation feature failed. <see cref="VideoWebhookPayload.Reason"/> names the feature
    /// (Title, Description, Chapters, or Moments). Sent once per failed feature.
    /// </summary>
    SmartGenerationFailed = 33,
}
