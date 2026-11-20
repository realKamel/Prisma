using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Abstractions.ObjectStorage;
using Prisma.Application.Abstractions.Security;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Extenions;
using Prisma.Application.Features.Lessons.Helpers;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.ObjectLibraryAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assistants;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.ObjectStorageSpec;

namespace Prisma.Application.Features.Lessons.Commands.AttachLessonAssignment;

internal sealed class AttachLessonAssignmentCommandHandler(
    ICurrentUserService currentUserService,
    IStorageService storageService,
    IUnitOfWork unitOfWork,
    IFileScanner fileScanner
) : ICommandHandler<AttachLessonAssignmentCommand, Result<AttachLessonAssignmentResponse>>
{
    public async Task<Result<AttachLessonAssignmentResponse>> Handle(
        AttachLessonAssignmentCommand request,
        CancellationToken cancellationToken
    )
    {
        if (!currentUserService.UserId.HasValue)
        {
            return Result.Unauthorized();
        }

        var userId = currentUserService.UserId.Value;

        Guid teacherId = Guid.Empty;

        // 1. Resolve Teacher ID based on Role
        if (currentUserService.Role == AppRoles.Teacher)
        {
            teacherId = userId;
        }
        else if (currentUserService.Role == AppRoles.Assistant)
        {
            var assistantRepo = unitOfWork.GetOrCreateRepository<Assistant, Guid>();
            var teacherAssignmentId = await assistantRepo.FirstOrDefaultAsync(
                new AssistantByIdWithProjectionSpec<Guid?>(userId, x => x.TeacherId),
                cancellationToken
            );

            if (teacherAssignmentId is null || teacherAssignmentId == Guid.Empty)
            {
                return Result.Error("This assistant is not assigned to a teacher.");
            }

            teacherId = teacherAssignmentId.Value;
        }
        else if (currentUserService.Role == AppRoles.Admin)
        {
            // If Admin creates it, ensure your command supports passing a TeacherId,
            // or handle it according to your admin business logic.
            return Result.Error("Admin lesson creation flow needs explicit teacher assignment.");
        }

        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();

        var lesson = await lessonRepo.FirstOrDefaultAsync(
            new LessonByPublicId(request.LessonId),
            cancellationToken
        );

        if (lesson is null)
        {
            return Result.NotFound("The Requested lesson doesn't exit");
        }

        var assignedId = Guid.CreateVersion7();
        var objKey = UrlsHelper.ConstructLessonAssignmentObjectKey(
            lessonId: request.LessonId,
            objectKey: assignedId
        );
        var objectStorageRepository = unitOfWork.GetOrCreateRepository<ObjectStorage, Guid>();

        var objectStorage = await objectStorageRepository.FirstOrDefaultAsync(
            new ObjectStorageByTeacherId(teacherId: teacherId),
            cancellationToken
        );

        if (objectStorage is null)
            return Result.NotFound();

        var attachingResult = objectStorage.AddFile(
            assignedId,
            objectKey: objKey,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length
        );

        if (!attachingResult.IsSuccess)
        {
            return attachingResult.Map();
        }

        var fileObject = attachingResult.Value;

        var addingResult = lesson.AddAssignment(
            fileObjectId: fileObject.Id,
            request.TotalMarks,
            request.AssignmentDueDate,
            request.File.FileName
        );

        if (!addingResult.IsSuccess)
        {
            return addingResult.Map();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // storageService.UploadFileAsync(objectStorage.BucketName,);

        var linkResult = await storageService.GetPreSignedUploadUrlAsync(
            bucket: objectStorage.BucketName,
            objectKey: objKey,
            1.DayInSeconds(),
            request.File.ContentType,
            cancellationToken: cancellationToken
        );

        if (!linkResult.IsSuccess)
        {
            return linkResult.Map();
        }

        var objId = lesson.Assignment.FileObject.ObjectKey;

        var rep = await objectStorageRepository.FirstOrDefaultAsync(
            new ObjectStorageByFileObjectIdSpec(lesson.Assignment.FileObjectId),
            cancellationToken
        );

        var link = await storageService.GetPreSignedDownloadUrlAsync(
            objectStorage.BucketName,
            objKey,
            1.DayInSeconds(),
            cancellationToken
        );

        return new AttachLessonAssignmentResponse(linkResult.Value);
    }
}
