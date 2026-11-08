using Prisma.Application.Abstractions.Encryption;
using Prisma.Application.Abstractions.VideoServices;
using Prisma.Domain.DomainEvents;
using Prisma.Domain.Entities.ObjectLibraryAggregate;
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

        if (library.ProvisioningStatus == ProvisioningStatus.Active)
        {
            return;
        }

        var result = await videoLibraryService.CreateLibrary(
            ObjectStorage.GenerateBucketName(teacherId: message.TeacherId),
            cancellationToken,
            EncodingTier.Free
        );

        if (!result.IsSuccess)
        {
            library.MarkAsFailed();
            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Failed to provision video library for teacher {message.TeacherId}."
            );
        }

        var libraryConfig = result.Value;
        var encryptedApiKey = secretCipher.Encrypt(libraryConfig.ApiKey);
        var encryptedReadonlyApiKey = secretCipher.Encrypt(libraryConfig.ReadOnlyApiKey);

        library.MarkLibraryAsActive(
            providerId: libraryConfig.ExternalLibraryId,
            libraryApiKey: encryptedApiKey,
            readOnlyApiKey: encryptedReadonlyApiKey,
            webhookUrl: libraryConfig.WebhookUrl
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
