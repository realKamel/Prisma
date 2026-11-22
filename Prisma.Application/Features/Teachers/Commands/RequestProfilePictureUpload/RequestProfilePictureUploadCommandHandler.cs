using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Abstractions.ObjectStorage;
using Prisma.Application.Features.Lessons.Helpers;
using Prisma.Domain.Aggregates.ObjectLibraryAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.ObjectStorageSpec;
using Prisma.Domain.Specifications.Teachers;

namespace Prisma.Application.Features.Teachers.Commands.RequestProfilePictureUpload;

internal sealed class RequestProfilePictureUploadCommandHandler(
    ICurrentUserService currentUserService,
    IStorageService storageService,
    IUnitOfWork unitOfWork
) : ICommandHandler<RequestProfilePictureUploadCommand, Result<RequestProfilePictureUploadResponse>>
{
    private const int UploadLinkLifetimeInSeconds = 15 * 60;

    public async Task<Result<RequestProfilePictureUploadResponse>> Handle(
        RequestProfilePictureUploadCommand request,
        CancellationToken cancellationToken
    )
    {
        if (currentUserService.UserId is not { } teacherId)
        {
            return Result.Unauthorized();
        }

        var teacherRepo = unitOfWork.GetOrCreateRepository<Teacher, Guid>();
        var teacher = await teacherRepo.FirstOrDefaultAsync(
            new TeacherForProfileSpec(teacherId),
            cancellationToken
        );

        if (teacher is null)
        {
            return Result.NotFound("Teacher not found.");
        }

        var objectStorageRepository = unitOfWork.GetOrCreateRepository<ObjectStorage, Guid>();
        var objectStorage = await objectStorageRepository.FirstOrDefaultAsync(
            new ObjectStorageByTeacherId(teacherId: teacherId),
            cancellationToken
        );

        if (objectStorage is null)
        {
            return Result.NotFound("The teacher's storage is not provisioned.");
        }

        var fileId = Guid.CreateVersion7();
        var objectKey = UrlsHelper.ConstructTeacherProfilePictureObjectKey(fileId);
        var contentType = request.ContentType.ToLowerInvariant();

        var attachingResult = objectStorage.AddFile(
            fileId,
            objectKey,
            request.FileName,
            contentType,
            request.Size
        );

        if (!attachingResult.IsSuccess)
        {
            return attachingResult.Map();
        }

        // Replaces the earlier picture. The browser's PUT to the link below is what makes the new one real.
        var replacedObjectKey = teacher.ProfileImageFile?.ObjectKey;
        teacher.SetProfileImage(attachingResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var linkResult = await storageService.GetPreSignedUploadUrlAsync(
            bucket: objectStorage.BucketName,
            objectKey: objectKey,
            UploadLinkLifetimeInSeconds,
            contentType,
            cancellationToken: cancellationToken
        );

        if (!linkResult.IsSuccess)
        {
            return linkResult.Map();
        }

        if (replacedObjectKey is not null)
        {
            await DeleteReplacedObjectAsync(
                objectStorage.BucketName,
                replacedObjectKey,
                cancellationToken
            );
        }

        return new RequestProfilePictureUploadResponse(fileId, linkResult.Value);
    }

    /// <summary>
    /// Removes the replaced picture's object from the bucket. Best effort: a leftover object is
    /// harmless and must not fail the upload request.
    /// </summary>
    private async Task DeleteReplacedObjectAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await storageService.DeleteFileAsync(bucketName, objectKey, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Swallowed on purpose; see the summary above.
        }
    }
}
