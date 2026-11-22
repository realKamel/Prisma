using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Abstractions.ObjectStorage;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Aggregates.FileObjectAggregate;
using Prisma.Domain.Aggregates.ObjectLibraryAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assistants;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.ObjectStorageSpec;

namespace Prisma.Application.Features.Lessons.Commands.UploadLessonMaterials;

internal sealed class UploadLessonMaterialsCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IStorageService storageService
) : ICommandHandler<UploadLessonMaterialsCommand, Result>
{
    public async Task<Result> Handle(
        UploadLessonMaterialsCommand request,
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
        else
        {
            // If Admin creates it, ensure your command supports passing a TeacherId,
            // or handle it according to your admin business logic.
            return Result.Error("Lesson creation flow needs explicit teacher assignment.");
        }

        var objectStorageRepository = unitOfWork.GetOrCreateRepository<ObjectStorage, Guid>();

        var objectStorage = await objectStorageRepository.FirstOrDefaultAsync(
            new ObjectStorageByTeacherId(teacherId: teacherId),
            cancellationToken
        );

        if (objectStorage is null)
            return Result.NotFound("There's no space for the teacher to upload to");

        var lessonRepository = unitOfWork.GetOrCreateRepository<Lesson, int>();

        var spec = new LessonWithMaterialsForUpdateSpecification(request.LessonId);

        var lesson = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (lesson is null)
        {
            return Result.NotFound($"Lesson with id '{request.LessonId}' was not found");
        }

        if (request.Files.Count == 0)
        {
            return Result.Error("No files provided for upload.");
        }

        //TODO: this is to satisfy the type system i don't know it it works at all
        foreach (var file in request.Files)
        {
            if (file.Length > 0)
            {
                var filename = $"material/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                using var stream = file.OpenReadStream();

                await storageService.UploadFileAsync(
                    storageService.DefaultBucketName,
                    filename,
                    stream,
                    file.ContentType,
                    cancellationToken
                );

                string fileSize =
                    file.Length < 1024 * 1024
                        ? $"{Math.Round((double)file.Length / 1024, 1)} KB"
                        : $"{Math.Round((double)file.Length / (1024 * 1024), 1)} MB";

                var ext = Path.GetExtension(file.FileName).ToLower();
                var materialType = ext switch
                {
                    ".pdf" => LessonMaterialType.PDF,
                    ".ppt" or ".pptx" => LessonMaterialType.PPT,
                    _ => LessonMaterialType.PDF,
                };
                var fileId = Guid.CreateVersion7();

                var fileObjectResult = FileObject.CreateWithCustomKey(
                    fileId,
                    objectStorage.Id,
                    filename,
                    file.FileName,
                    file.ContentType,
                    file.Length
                );

                lesson.LessonMaterials.Add(
                    new LessonMaterial
                    {
                        // Title = Path.GetFileNameWithoutExtension(file.FileName),
                        // Size = fileSize,
                        // Type = materialType,
                        // DownloadUrl = filename,
                        FileObject = fileObjectResult.Value,
                        LessonId = lesson.Id,
                    }
                );
            }
        }

        // lessonRepository.Update(lesson);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessWithMessage("Materials uploaded and attached to lesson successfully");
    }
}
