using Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.CleanUpUnconfiguredResolutions;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.CreateVideo;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.GetVideo;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.GetVideos;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.GetVideoStorageSizeInfo;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.UpdateVideo;
using Refit;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Stream;

public interface IBunnyStreamVideosApi
{
    /// <summary>
    /// Retrieves the detailed configuration, metadata, and encoding status of a specific video.
    /// </summary>
    /// <param name="libraryId">The ID of the video library.</param>
    /// <param name="videoId">The unique GUID of the video.</param>
    /// <param name="libraryApiKey">The API key for the same library.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The details of the requested video.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if libraryId is less than or equal to 0, or videoId is null/empty.</exception>
    /// <exception cref="ApiException">Thrown when the API returns an error status code (e.g., 401 Unauthorized, 404 Not Found).</exception>
    [Get("/library/{libraryId}/videos/{videoId}")]
    Task<GetVideoResponse> GetVideo(
        [AliasAs("libraryId")] long libraryId,
        [AliasAs("videoId")] string videoId,
        [Header("Authorization")] string libraryApiKey,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Creates a new video entry in the specified library, returning a video GUID that can be used for subsequent uploads.
    /// </summary>
    /// <param name="libraryId">The ID of the video library.</param>
    /// <param name="libraryApiKey">The API key for the same library.</param>
    /// <param name="model">The configuration details for the new video. The 'Title' property is required.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The newly created VideoModel, including its unique GUID and initial status.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if libraryId is less than or equal to 0.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the model is null.</exception>
    /// <exception cref="ApiException">Thrown when the API returns an error status code (400, 401, 403, 500).</exception>
    [Post("/library/{libraryId}/videos")]
    Task<CreateVideoResponse> CreateVideo(
        [AliasAs("libraryId")] long libraryId,
        [Header("Authorization")] string libraryApiKey,
        [Body] CreateVideoRequest model,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Updates the metadata of an existing video.
    /// </summary>
    /// <param name="libraryId">The ID of the video library.</param>
    /// <param name="videoId">The unique GUID of the video.</param>
    /// <param name="libraryApiKey">The API key for the same library.</param>
    /// <param name="model">The properties to update. Only non-null properties will be applied.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A StatusModel indicating success or failure.</returns>
    /// <exception cref="ApiException">Thrown when the API returns an error status code (401, 403, 404, 500).</exception>
    [Post("/library/{libraryId}/videos/{videoId}")]
    Task<BunnyStreamGeneralResponse> UpdateVideo(
        [AliasAs("libraryId")] long libraryId,
        [AliasAs("videoId")] string videoId,
        [Header("Authorization")] string libraryApiKey,
        [Body] UpdateVideoRequest model,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Permanently deletes a video from the library.
    /// </summary>
    /// <param name="libraryId">The ID of the video library.</param>
    /// <param name="videoId">The unique GUID of the video to delete.</param>
    /// <param name="libraryApiKey">The API key for the same library.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A StatusModel indicating success or failure.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if libraryId is less than or equal to 0, or videoId is null/empty.</exception>
    /// <exception cref="ApiException">Thrown when the API returns an error status code (401, 403, 404, 500).</exception>
    /// <remarks>
    /// ⚠️ WARNING: This action cannot be undone. The video stops appearing in lookups immediately,
    /// but playback may still be possible for a short time via CDN caching before access is fully revoked.
    /// </remarks>
    [Delete("/library/{libraryId}/videos/{videoId}")]
    Task<BunnyStreamGeneralResponse> DeleteVideo(
        [AliasAs("libraryId")] long libraryId,
        [AliasAs("videoId")] string videoId,
        [Header("Authorization")] string libraryApiKey,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Retrieves a paginated list of videos within a specific video library.
    /// </summary>
    /// <param name="libraryId">The ID of the video library.</param>
    /// <param name="libraryApiKey">The API key for the same library.</param>
    /// <param name="page">The page number to return, 1-based. Values below 1 are treated as 1. (Default: 1)</param>
    /// <param name="itemsPerPage">The number of items to return per page, clamped to a range of 10-1000. (Default: 100)</param>
    /// <param name="search">Optional. Filters videos by title, case-insensitive substring match.</param>
    /// <param name="collection">Optional. Filters videos by collection ID.</param>
    /// <param name="orderBy">Optional. The field to order results by. Possible values: "date", "title". Any other value falls back to "date". (Default: "date")</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A paginated list of VideoModel objects.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if libraryId is less than or equal to 0.</exception>
    /// <exception cref="ApiException">Thrown when the API returns an error status code (400, 401, 500, 503).</exception>
    [Get("/library/{libraryId}/videos")]
    Task<GetVideosResponse> ListVideos(
        [AliasAs("libraryId")] long libraryId,
        [Header("Authorization")] string libraryApiKey,
        [Query] int? page = null,
        [Query] int? itemsPerPage = null,
        [Query] string? search = null,
        [Query] string? collection = null,
        [Query] string? orderBy = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves detailed storage size information for a specific video, broken down by rendition, thumbnails, originals, etc.
    /// </summary>
    /// <param name="libraryId">The ID of the video library.</param>
    /// <param name="videoId">The unique GUID of the video.</param>
    /// <param name="libraryApiKey">The API key for the same library.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A StatusModelOfVideoStorageSizeModel containing the storage breakdown.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if libraryId is less than or equal to 0, or videoId is null/empty.</exception>
    /// <exception cref="ApiException">Thrown when the API returns an error status code (401, 404, 408, 429, 500).</exception>
    [Get("/library/{libraryId}/videos/{videoId}/storage")]
    Task<GetVideoStorageSizeInfoResponse> GetVideoStorageSize(
        [AliasAs("libraryId")] long libraryId,
        [AliasAs("videoId")] string videoId,
        [Header("Authorization")] string libraryApiKey,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Deletes stored resolution files that are no longer configured for the library, or specific resolutions passed.
    /// </summary>
    /// <param name="libraryId">The ID of the video library.</param>
    /// <param name="videoId">The unique GUID of the video.</param>
    /// <param name="libraryApiKey">The API key for the same library.</param>
    /// <param name="resolutionsToDelete">Comma-separated list of specific resolutions to delete (e.g., "720p,480p").</param>
    /// <param name="deleteNonConfiguredResolutions">If true, all resolutions not currently configured for the library will be deleted.</param>
    /// <param name="allResolutions">If true, all resolutions for the video will be deleted, regardless of library configuration.</param>
    /// <param name="deleteOriginal">If true, the original source file is deleted as well.</param>
    /// <param name="outputs">Outputs to clean. Supported values: "hls", "mp4", "all".</param>
    /// <param name="deleteMp4Files">If true, MP4 fallback files for the deleted resolutions are removed too.</param>
    /// <param name="dryRun">If true, no actual file manipulation will happen; only informational data will be returned.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A StatusModelOfResolutionsCleanupModel containing the cleanup plan or result.</returns>
    /// <exception cref="ApiException">Thrown when the API returns an error status code (400, 401, 403, 404, 500).</exception>
    /// <remarks>
    /// ⚠️ WARNING: This action cannot be undone.
    /// If the library has 'keep original files' turned off, the original source file is also deleted as part of this
    /// cleanup even when deleteOriginal is not explicitly set to true.
    ///
    /// When targeting HLS resolutions (the default, or outputs=all), at least one resolution must remain after cleanup
    /// or the request is rejected (400). To remove every rendition, scope outputs to "mp4".
    /// </remarks>
    [Post("/library/{libraryId}/videos/{videoId}/resolutions/cleanup")]
    Task<CleanUpUnconfiguredResolutionsResponse> CleanupVideoResolutions(
        [AliasAs("libraryId")] long libraryId,
        [AliasAs("videoId")] string videoId,
        [Header("Authorization")] string libraryApiKey,
        [Query] string? resolutionsToDelete = null,
        [Query] bool? deleteNonConfiguredResolutions = null,
        [Query] bool? allResolutions = null,
        [Query] bool? deleteOriginal = null,
        [Query] string? outputs = null,
        [Query] bool? deleteMp4Files = null,
        [Query] bool? dryRun = null,
        CancellationToken cancellationToken = default
    );
}
