using Prisma.Application.Abstractions.Services.VideoServices;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Core;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Core.AddVideoLibrary;

namespace Prisma.Infrastructure.Services.VideoStreamingAndStorage;

internal sealed class VideoLibraryService(IBunnyCoreVideoLibraryApi api) : IVideoLibraryService
{
    public async Task<VideoLibrary> CreateLibrary(string name, CancellationToken cancellationToken)
    {
        var request = new AddVideoLibraryRequest
        {
            Name = name,
            EncodingTier = EncodingTier.Free,
            EnabledResolutions = "480p,720p,1080p",
            AllowDirectPlay = false,
        };
        var result = await api.AddVideoLibrary(request, cancellationToken);
        var createLibraryResult = VideoLibrary.Create(
            result.Id,
            result.ApiKey,
            result.ReadOnlyApiKey,
            result.WebhookUrl
        );
        return createLibraryResult;
    }
}
