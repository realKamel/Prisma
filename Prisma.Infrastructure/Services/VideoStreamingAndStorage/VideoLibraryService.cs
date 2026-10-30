using Prisma.Application.Abstractions.Services.VideoServices;
using Prisma.Application.Common.Interfaces;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Core;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Core.AddVideoLibrary;

namespace Prisma.Infrastructure.Services.VideoStreamingAndStorage;

internal sealed class VideoLibraryService(IBunnyCoreVideoLibraryApi api, ISecretCipher secretCipher)
    : IVideoLibraryService
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

        ArgumentException.ThrowIfNullOrWhiteSpace(result.ApiKey);
        // ArgumentException.ThrowIfNullOrWhiteSpace(result.ApiKey);
        var encryptedApiKey = secretCipher.Encrypt(result.ApiKey);
        var encryptedReadOnlyApiKey = secretCipher.Encrypt(result.ReadOnlyApiKey);

        var createLibraryResult = VideoLibrary.Create(
            result.Id,
            encryptedApiKey,
            encryptedReadOnlyApiKey,
            result.WebhookUrl
        );
        return createLibraryResult;
    }
}
