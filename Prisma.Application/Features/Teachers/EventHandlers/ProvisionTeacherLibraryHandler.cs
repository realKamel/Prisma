using Prisma.Application.Abstractions.Encryption;
using Prisma.Application.Abstractions.VideoServices;
using Prisma.Domain.Aggregates.VideoLibraryAggregate;
using Prisma.Domain.DomainEvents;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.VideoLibrarySpecs;

namespace Prisma.Application.Features.Teachers.EventHandlers;

public static class ProvisionTeacherLibraryHandler
{
    public static async Task Handle(
        TeacherCreatedEvent message,
        IUnitOfWork unitOfWork,
        ISecretCipher secretCipher,
        IVideoLibraryService videoLibraryService,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(secretCipher);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(videoLibraryService);

        var repository = unitOfWork.GetOrCreateRepository<VideoLibrary, int>();

        var library = await repository.FirstOrDefaultAsync(
            new VideoLibraryByTeacherId(message.TeacherId),
            cancellationToken
        );

        if (library is null)
        {
            throw new InvalidOperationException(
                $"Video library not found for teacher {message.TeacherId}."
            );
        }

        if (library.Status.IsReady)
        {
            return;
        }

        if (library.ProviderLibraryId is null)
        {
            var result = await videoLibraryService.CreateLibrary(
                VideoLibrary.GenerateLibraryName(message.TeacherId),
                EncodingTier.Free,
                enabledResolutions: "480p,720p,1080p",
                cancellationToken: cancellationToken
            );

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(
                    $"Failed to create video library for teacher {message.TeacherId}."
                );
            }

            var config = result.Value;

            library.ConfigureProviderCredentials(
                config.ExternalLibraryId,
                secretCipher.Encrypt(config.ApiKey),
                secretCipher.Encrypt(config.ReadOnlyApiKey)
            );

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var webhookResult = await videoLibraryService.ConfigureWebhookAsync(
            library.ProviderLibraryId!.Value,
            cancellationToken
        );

        if (!webhookResult.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Failed to configure webhook for teacher {message.TeacherId}."
            );
        }

        library.MarkAsActive();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
