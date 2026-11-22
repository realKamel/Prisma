using Microsoft.Extensions.Logging;
using Prisma.Application.Abstractions.ObjectStorage;
using Prisma.Domain.Aggregates.ObjectLibraryAggregate;
using Prisma.Domain.DomainEvents.LessonEvents;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.ObjectStorageSpec;

namespace Prisma.Application.Features.Lessons.EventHandlers;

public static partial class LessonMaterialRemovedHandler
{
    public static async Task Handle(
        LessonMaterialRemovedDomainEvent message,
        IStorageService storageService,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken cancellationToken
    )
    {
        // 1. Look up the storage that contains the file.
        var library = await unitOfWork
            .GetOrCreateRepository<ObjectStorage, Guid>()
            .FirstOrDefaultAsync(
                new ObjectStorageByFileObjectIdSpec(message.FileObjectId),
                cancellationToken
            );

        if (library is null)
        {
            // Critical data inconsistency: event says a file was removed,
            // but no storage owns it. We can't find bucket/key → can't clean up.
            LogOrphanedBlob(logger, message.FileObjectId);
            return;
        }

        // 2. Attempt to remove the file record from the aggregate.
        var removeResult = library.RemoveFile(message.FileObjectId);

        if (!removeResult.IsSuccess)
        {
            // "Storage not Ready" — retrying won't help. Log and complete.
            LogCannotRemoveFile(
                logger,
                message.FileObjectId,
                library.Id,
                string.Join("; ", removeResult.Errors)
            );
            return;
        }

        var key = removeResult.Value;
        if (key is null)
        {
            // File already absent from the aggregate. Idempotent no-op.
            return;
        }

        // 3. Delete blob FIRST (idempotent — missing key must not throw).
        await storageService.DeleteObjectAsync(library.BucketName, key, cancellationToken);

        // 4. Persist the removal. Concurrency exceptions bubble up to Wolverine's
        //    retry policy — do not catch here.
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    [LoggerMessage(
        EventId = 4101,
        Level = LogLevel.Error,
        Message = "Orphaned blob detected: No ObjectStorage found for FileObjectId "
            + "{FileObjectId}. Blob cleanup is impossible and requires manual intervention"
    )]
    private static partial void LogOrphanedBlob(ILogger logger, Guid fileObjectId);

    [LoggerMessage(
        EventId = 4102,
        Level = LogLevel.Warning,
        Message = "Cannot remove file {FileObjectId} from storage {StorageId}: {Errors}"
    )]
    private static partial void LogCannotRemoveFile(
        ILogger logger,
        Guid fileObjectId,
        Guid storageId,
        string errors
    );
}
