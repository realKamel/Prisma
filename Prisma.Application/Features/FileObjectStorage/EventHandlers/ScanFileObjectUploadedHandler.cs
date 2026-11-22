using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prisma.Application.Abstractions.ObjectStorage;
using Prisma.Application.Abstractions.Security;
using Prisma.Domain.Aggregates.FileObjectAggregate;
using Prisma.Domain.Aggregates.FileObjectAggregate.Specifications;
using Prisma.Domain.DomainEvents.ObjectStorage;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.FileObjectStorage.EventHandlers;

public static partial class ScanFileObjectUploadedHandler
{
    public static async Task Handle(
        FileObjectUploaded message,
        IStorageObjectService storage,
        IFileScanner scanner,
        IUnitOfWork unitOfWork,
        ILogger logger,
        TimeProvider timeProvider,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(scanner);
        ArgumentNullException.ThrowIfNull(timeProvider);

        var fileRepo = unitOfWork.GetOrCreateRepository<FileObject, Guid>();

        var file = await fileRepo.FirstOrDefaultAsync(
            new FileObjectAndLibraryObjectSpec(message.ObjectId),
            cancellationToken
        );

        if (file is null || !file.FileStatus.CanStartScanning)
        {
            return;
        }

        // 1. Claim the file.
        file.MarkAsScanning(timeProvider.GetUtcNow());

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return;
        }

        // 2. Download and scan.
        Result result;

        try
        {
            await using var stream = await storage.GetObjectAsync(
                file.ObjectStorage.BucketName,
                file.ObjectKey,
                cancellationToken
            );

            result = await scanner.ScanAsync(stream, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogFailedToScanFileFileid(file.Id, exception);

            file.MarkScanAsFailed();
            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw;
        }

        // 3. Persist the scan result.
        var scannedAt = timeProvider.GetUtcNow();

        if (result.IsSuccess)
        {
            file.MarkAsAvailable(scannedAt);
        }
        else
        {
            file.MarkAsRejected(scannedAt);
            logger.LogMalwareDetectedInFileFileid(file.Id);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    [LoggerMessage(LogLevel.Warning, "Malware detected in file {FileId}")]
    private static partial void LogMalwareDetectedInFileFileid(this ILogger logger, Guid fileId);

    [LoggerMessage(LogLevel.Error, "Failed to scan file {FileId}")]
    private static partial void LogFailedToScanFileFileid(
        this ILogger logger,
        Guid fileId,
        Exception exception
    );
}
