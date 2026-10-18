using System.Text.Json.Serialization;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.CleanUpUnconfiguredResolutions;

/// <summary>
/// Specialized status response that includes video resolutions cleanup data.
/// Inherits base properties (Success, Message, StatusCode) from StatusModel.
/// </summary>
public record CleanUpUnconfiguredResolutionsResponse : BunnyStreamGeneralResponse
{
    /// <summary>
    /// The response payload containing the cleanup plan or result.
    /// Populated even on 400 Bad Request to show what was evaluated up to the rejection point.
    /// </summary>
    [JsonPropertyName("data")]
    public ResolutionsCleanupModel? Data { get; init; }
}

/// <summary>
/// Detailed breakdown of the resolutions' cleanup operation.
/// </summary>
public record ResolutionsCleanupModel
{
    [JsonPropertyName("videoId")]
    public string? VideoId { get; init; }

    [JsonPropertyName("videoLibraryId")]
    public long VideoLibraryId { get; init; }

    /// <summary>
    /// Resolutions selected for removal as part of this cleanup, across the HLS playlist, object storage, and MP4 fallback set.
    /// </summary>
    [JsonPropertyName("resolutionsToDelete")]
    public List<string>? ResolutionsToDelete { get; init; }

    /// <summary>
    /// Resolutions selected for removal from the HLS master playlist.
    /// </summary>
    [JsonPropertyName("resolutionsToDeleteFromPlaylist")]
    public List<string>? ResolutionsToDeleteFromPlaylist { get; init; }

    /// <summary>
    /// Resolutions selected for removal from object storage.
    /// </summary>
    [JsonPropertyName("resolutionsToDeleteFromStorage")]
    public List<string>? ResolutionsToDeleteFromStorage { get; init; }

    /// <summary>
    /// Resolutions selected for removal from the MP4 fallback set.
    /// </summary>
    [JsonPropertyName("resolutionsToDeleteFromMp4")]
    public List<string>? ResolutionsToDeleteFromMp4 { get; init; }

    /// <summary>
    /// Storage object paths selected for deletion as part of this cleanup.
    /// These are candidates; see DeletedStorageObjects for whether at least one deletion actually succeeded.
    /// </summary>
    [JsonPropertyName("storageObjectsToDelete")]
    public List<string>? StorageObjectsToDelete { get; init; }

    /// <summary>
    /// The video's available resolutions after this cleanup.
    /// </summary>
    [JsonPropertyName("availableResolutionsAfter")]
    public List<string>? AvailableResolutionsAfter { get; init; }

    [JsonPropertyName("updatedAvailableResolutions")]
    public bool UpdatedAvailableResolutions { get; init; }

    [JsonPropertyName("modifiedPlaylist")]
    public bool ModifiedPlaylist { get; init; }

    /// <summary>
    /// Whether at least one of the storage objects was actually deleted (always false when dryRun is true).
    /// </summary>
    [JsonPropertyName("deletedStorageObjects")]
    public bool DeletedStorageObjects { get; init; }

    /// <summary>
    /// Whether the original uploaded source file was deleted from storage as part of this operation.
    /// </summary>
    [JsonPropertyName("deletedOriginal")]
    public bool DeletedOriginal { get; init; }
}
