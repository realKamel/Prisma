using Ardalis.Result;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Abstractions.VideoServices;

/// <summary>
/// Provides operations for managing video libraries on the external streaming provider (Bunny).
/// </summary>
public interface IVideoLibraryService
{
    /// <summary>
    /// Creates a new video library on the external provider (Bunny) and returns its credentials.
    /// </summary>
    /// <param name="name">The display name of the video library.</param>
    /// <param name="encodingTier">
    /// The encoding tier for the library. <see cref="EncodingTier.Free"/> maps to the provider's
    /// free tier; all other values map to the premium tier.
    /// </param>
    /// <param name="enabledResolutions">
    /// The provider-formatted list of enabled resolutions (e.g. "480p,720p,1080p").
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A successful <see cref="Result{VideoLibraryCreateResult}"/> containing the provider library ID,
    /// API keys, and webhook URL; or a failed result if the provider call fails.
    /// </returns>
    Task<Result<VideoLibraryCreateResult>> CreateLibrary(
        string name,
        EncodingTier encodingTier,
        string enabledResolutions,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Configures the webhook URL on an existing provider video library.
    /// The webhook URL is read from <c>BunnyDotNetOptions.LibraryStreamWebhookUrl</c> and must be a valid HTTPS URL.
    /// </summary>
    /// <param name="providerLibraryId">The provider's video library identifier. Must be greater than zero.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A successful <see cref="Result"/> if the webhook was configured; or a failed result if the provider call fails.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="providerLibraryId"/> is zero or negative.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the configured webhook URL is missing, not absolute, or not HTTPS.
    /// </exception>
    Task<Result> ConfigureWebhookAsync(
        long providerLibraryId,
        CancellationToken cancellationToken = default
    );
}
