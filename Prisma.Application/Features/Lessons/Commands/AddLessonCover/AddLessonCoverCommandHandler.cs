using Ardalis.Result;
using Microsoft.Extensions.Logging;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Abstractions.ObjectStorage;
using Prisma.Application.Abstractions.Security;
using Prisma.Application.Features.Lessons.Helpers;
using Prisma.Domain.Aggregates.ObjectLibraryAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.ObjectStorageSpec;

namespace Prisma.Application.Features.Lessons.Commands.AddLessonCover;

public sealed partial class AddLessonCoverCommandHandler(
    IFileScanner scanner,
    ICurrentUserService currentUserService,
    IStorageService storageService,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork,
    ILogger<AddLessonCoverCommandHandler> logger
) : ICommandHandler<AddLessonCoverCommand, Result>
{
    public async Task<Result> Handle(
        AddLessonCoverCommand request,
        CancellationToken cancellationToken
    )
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.WorkspaceId.HasValue)
        {
            return Result.Unauthorized();
        }

        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();

        var lesson = await lessonRepo.FirstOrDefaultAsync(
            new LessonByPublicId(request.PublicLessonId),
            cancellationToken
        );

        if (lesson is null)
        {
            return Result.NotFound();
        }

        var objectLibraryRepo = unitOfWork.GetOrCreateRepository<ObjectStorage, Guid>();

        var objectStorage = await objectLibraryRepo.FirstOrDefaultAsync(
            new ObjectStorageByWorkspaceIdSpec(workspaceId: currentUserService.WorkspaceId.Value),
            cancellationToken
        );

        if (objectStorage is null)
        {
            return Result.NotFound();
        }

        if (!request.Content.CanSeek)
        {
            return Result.Error("The uploaded stream must support seeking.");
        }

        var scanResult = await scanner.ScanAsync(request.Content, cancellationToken);

        if (!scanResult.IsSuccess)
        {
            return scanResult.Map();
        }

        var fileId = Guid.CreateVersion7();

        var objectKey = UrlsHelper.ConstructLessonCoverFileObjectKey(
            lessonId: request.PublicLessonId,
            fileId
        );

        var objectStorageAddFileResult = objectStorage.AddFile(
            fileId,
            objectKey,
            request.FileDto.FileName,
            request.FileDto.ContentType,
            request.FileDto.Size
        );

        if (!objectStorageAddFileResult.IsSuccess)
        {
            return objectStorageAddFileResult.Map();
        }

        request.Content.Seek(0, SeekOrigin.Begin);
        request.Content.Position = 0;
        try
        {
            await storageService.UploadFileAsync(
                objectStorage.BucketName,
                objectKey,
                request.Content,
                request.FileDto.ContentType,
                cancellationToken
            );

            lesson.SetCover(fileId);

            objectStorageAddFileResult.Value.MarkAsAvailable(timeProvider.GetUtcNow());

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            try
            {
                await storageService.DeleteFileAsync(
                    objectStorage.BucketName,
                    objectKey,
                    CancellationToken.None
                );
            }
            catch (Exception cleanupException)
            {
                FailedToCleanupUploadedLessonCover(logger, cleanupException, fileId);
            }

            FailedToUploadLessonCoverForLesson(logger, exception, request.PublicLessonId);

            return Result.CriticalError(["Failed to upload lesson cover."]);
        }

        return Result.NoContent();
    }

    [LoggerMessage(
        EventId = 4200,
        Level = LogLevel.Error,
        Message = "Failed to clean up uploaded lesson cover {FileObjectId}"
    )]
    public static partial void FailedToCleanupUploadedLessonCover(
        ILogger logger,
        Exception exception,
        Guid fileObjectId
    );

    [LoggerMessage(
        EventId = 4201,
        Level = LogLevel.Error,
        Message = "Failed to upload lesson cover for lesson {LessonId}"
    )]
    public static partial void FailedToUploadLessonCoverForLesson(
        ILogger logger,
        Exception exception,
        Guid lessonId
    );
}
