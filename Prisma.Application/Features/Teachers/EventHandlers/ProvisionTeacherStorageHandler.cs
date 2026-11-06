using Prisma.Application.Abstractions.Services;
using Prisma.Application.Abstractions.Services.ObjectStorage;
using Prisma.Domain.DomainEvents;
using Prisma.Domain.Entities.ObjectLibraryAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.ObjectStorageSpec;

namespace Prisma.Application.Features.Teachers.EventHandlers;

public static class ProvisionTeacherObjectStorageHandler
{
    public static async Task Handle(
        TeacherCreatedEvent message,
        IUnitOfWork unitOfWork,
        IStorageService storageService,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(storageService);

        var objectStorageRepo = unitOfWork.GetOrCreateRepository<ObjectStorage, Guid>();

        var storage = await objectStorageRepo.FirstOrDefaultAsync(
            new ObjectStorageByTeacherId(message.TeacherId),
            cancellationToken
        );

        if (storage is null)
        {
            throw new InvalidOperationException(
                "Teacher's Object Storage isn't found. Probably a problem happened in teacher onboarding"
            );
        }

        if (storage.Status == StorageStatus.Ready)
        {
            return;
        }

        var result = await storageService.CreateBucketAsync(storage.BucketName, cancellationToken);

        if (!result.IsSuccess)
        {
            storage.MarkAsFailed();

            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Surface the failure so Wolverine can retry if appropriate.
            throw new InvalidOperationException(
                $"Failed to provision object storage for teacher {message.TeacherId}."
            );
        }

        storage.MarkAsProvisioned(StorageProvider.CloudflareR2);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
