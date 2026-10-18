using System.Text.Json.Serialization;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.Webhooks;

/// <summary>
/// Represents the payload of a Bunny.net Stream webhook notification.
/// </summary>
public sealed record VideoWebhookPayload
{
    [JsonPropertyName("VideoLibraryId")]
    public long VideoLibraryId { get; init; }

    [JsonPropertyName("VideoGuid")]
    public string VideoGuid { get; init; } = string.Empty;

    [JsonPropertyName("Status")]
    public VideoWebhookStatus Status { get; init; }
}

/// <summary>
/// Represents the possible status codes sent with the Bunny.net Stream webhook.
/// </summary>
public enum VideoWebhookStatus
{
    Queued = 0,
    Processing = 1,
    Encoding = 2,
    Finished = 3,
    ResolutionFinished = 4,
    Failed = 5,
    PresignedUploadStarted = 6,
    PresignedUploadFinished = 7,
    PresignedUploadFailed = 8,
    CaptionsGenerated = 9,
    TitleOrDescriptionGenerated = 10,
}
