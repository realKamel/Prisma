using Prisma.Domain.DomainEvents.LessonEvents;
using Prisma.Domain.Entities.ObjectLibraryAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.ObjectStorageSpec;

namespace Prisma.Application.Features.Lessons.EventHandlers;

public static class LessonAssignmentRemovedHandler
{
    public static async Task Handle(
        LessonAssignmentRemoved message,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken
    )
    {
        var objectStorageRepo = unitOfWork.GetOrCreateRepository<ObjectStorage, Guid>();

        var objectStorage = await objectStorageRepo.FirstOrDefaultAsync(
            new ObjectStorageByFileObjectIdSpec(message.FileObjectId),
            cancellationToken
        );
        if (objectStorage is null)
            throw new InvalidOperationException("Object storage was not found.");

        var bucketName = objectStorage.BucketName;

        var fileObject = objectStorage.Files.FirstOrDefault(x => x.Id == message.FileObjectId);

        if (fileObject is null)
        {
            return;
        }

        var result = objectStorage.RemoveFile(message.FileObjectId);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException("Failed to remove the file from object storage.");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
