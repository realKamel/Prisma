using Ardalis.Result;
using Prisma.Application.Abstractions.VideoServices;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Core;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Core.AddVideoLibrary;
using Prisma.Infrastructure.Helpers;
using EncodingTier = Prisma.Domain.Entities.UserAggregate.EncodingTier;

namespace Prisma.Infrastructure.Services.VideoStreamingAndStorage;

public sealed class VideoLibraryService(IBunnyCoreVideoLibraryApi api) : IVideoLibraryService
{
    public async Task<Result<VideoLibraryCreateResult>> CreateLibrary(
        string name,
        CancellationToken cancellationToken,
        EncodingTier encodingTier,
        string enabledResolutions = "480p,720p,1080p"
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

            return new VideoLibraryCreateResult(
                result.Id,
                result.ApiKey,
                result.ReadOnlyApiKey,
                result.WebhookUrl
            );
        }
        catch (HttpRequestException httpRequestException)
        {
            return httpRequestException.ToResult("video Library");
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<Result<VideoLibrary>> EnsureLibraryExistsAsync(
        string libraryName,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }
}
