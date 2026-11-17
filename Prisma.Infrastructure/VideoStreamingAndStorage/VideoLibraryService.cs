using Ardalis.Result;
using Microsoft.Extensions.Options;
using Prisma.Application.Abstractions.VideoServices;
using Prisma.Application.Common.Options;
using Prisma.Infrastructure.ExternalServices.BunnyApi;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Core;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Core.AddVideoLibrary;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Core.UpdateVideoLibrary;
using Prisma.Infrastructure.Helpers;
using EncodingTier = Prisma.Domain.Entities.UserAggregate.EncodingTier;

namespace Prisma.Infrastructure.VideoStreamingAndStorage;

public sealed class VideoLibraryService(
    IBunnyCoreVideoLibraryApi api,
    IOptions<BunnyDotNetOptions> bunnyOptions,
    IOptions<ApplicationUrlsOptions> appUrls
) : IVideoLibraryService
{
    /// <summary>
    /// Creates a new video library on the external provider (Bunny).
    /// </summary>
    /// <remarks>
    /// Behavior:
    /// <list type="bullet">
    ///   <item>Maps <see cref="EncodingTier.Free"/> to the provider's free tier; any other value maps to premium.</item>
    ///   <item>Sends the request with <c>AllowDirectPlay = false</c>.</item>
    ///   <item>Validates that the provider returned non-empty <c>ApiKey</c> and <c>ReadOnlyApiKey</c>; throws <see cref="ArgumentException"/> otherwise.</item>
    ///   <item>Catches <see cref="HttpRequestException"/> and converts it to a failed <see cref="Result{VideoLibraryCreateResult}"/>.</item>
    /// </list>
    /// </remarks>
    /// <param name="name">The display name of the video library.</param>
    /// <param name="encodingTier">The encoding tier to request from the provider.</param>
    /// <param name="enabledResolutions">The provider-formatted list of enabled resolutions.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A successful result containing the provider library ID, API keys, and webhook URL,
    /// or a failed result when the HTTP request fails.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provider response is missing <c>ApiKey</c> or <c>ReadOnlyApiKey</c>.
    /// </exception>
    /// <exception cref="HttpRequestException">
    /// Caught and translated into a failed <see cref="Result{VideoLibraryCreateResult}"/>; not rethrown.
    /// </exception>
    public async Task<Result<VideoLibraryCreateResult>> CreateLibrary(
        string name,
        EncodingTier encodingTier,
        string enabledResolutions,
        CancellationToken cancellationToken
    )
    {
        ExternalServices.BunnyApi.Core.AddVideoLibrary.EncodingTier? mappedEncodingTier =
            encodingTier == EncodingTier.Free
                ? ExternalServices.BunnyApi.Core.AddVideoLibrary.EncodingTier.Free
                : ExternalServices.BunnyApi.Core.AddVideoLibrary.EncodingTier.Premium;

        var request = new AddVideoLibraryRequest
        {
            Name = name,
            EncodingTier = mappedEncodingTier,
            EnabledResolutions = enabledResolutions,
            AllowDirectPlay = false,
        };

        try
        {
            var result = await api.AddVideoLibrary(request, cancellationToken);

            ArgumentException.ThrowIfNullOrWhiteSpace(result.ApiKey);
            ArgumentException.ThrowIfNullOrWhiteSpace(result.ReadOnlyApiKey);

            return new VideoLibraryCreateResult(
                result.Id,
                result.ApiKey,
                result.ReadOnlyApiKey,
                result.WebhookUrl
            );
        }
        catch (HttpRequestException httpRequestException)
        {
            return httpRequestException.ToResult("Video Library");
        }
    }

    /// <summary>
    /// Configures the webhook URL on an existing provider video library.
    /// </summary>
    /// <remarks>
    /// The webhook URL is sourced from <c>BunnyDotNetOptions.LibraryStreamWebhookUrl</c>.
    /// It must be an absolute, HTTPS URL; otherwise an <see cref="ArgumentException"/> is thrown.
    /// <see cref="HttpRequestException"/> raised by the provider is translated into a failed <see cref="Result"/>.
    /// </remarks>
    /// <param name="providerLibraryId">The provider's video library identifier. Must be greater than zero.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when the webhook update succeeds,
    /// or a failed result when the HTTP request fails.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="providerLibraryId"/> is zero or negative.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the injected <see cref="IOptions{BunnyDotNetOptions}"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the configured webhook URL is missing, not absolute, or not HTTPS.
    /// </exception>
    public async Task<Result> ConfigureWebhookAsync(
        long providerLibraryId,
        CancellationToken cancellationToken
    )
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(providerLibraryId);

        // validate the composed URL, not the raw path fragment.
        // Backend(path) returns Uri already (see ApplicationUrlsOptions)
        // the factory throws if the composition is invalid. We only need to assert the scheme.
        var url = appUrls.Value.Backend(bunnyOptions.Value.LibraryStreamWebhookPath);

        // http/Https both parse as valid absolute URIs. A webhook must be https.
        if (url.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException(
                $"Webhook URL must use HTTPS. Got: {url.AbsoluteUri}",
                nameof(bunnyOptions)
            );
        }

        var request = new UpdateVideoLibraryRequest
        {
            // AbsoluteUri, not ToString() — ToString() is display-oriented and may
            // un-escape chars, producing an invalid URL on the wire.
            WebhookUrl = url.AbsoluteUri,
        };

        try
        {
            await api.UpdateVideoLibrary(providerLibraryId, request, cancellationToken);
            return Result.Success();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToResult("Video Library Webhook");
        }
    }
}
