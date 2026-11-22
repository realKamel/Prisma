using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Abstractions.ObjectStorage;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Extenions;
using Prisma.Application.Features.Lessons.Helpers;
using Prisma.Domain.Aggregates.ObjectLibraryAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assistants;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.ObjectStorageSpec;

namespace Prisma.Application.Features.Lessons.Commands.AddLessonMaterial;

internal sealed class AddLessonMaterialCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IStorageService storageService
) : ICommandHandler<AddLessonMaterialCommand, Result<AddLessonMaterialCommandResponse>>
{
    public async Task<Result<AddLessonMaterialCommandResponse>> Handle(
        AddLessonMaterialCommand request,
        CancellationToken cancellationToken
    )
    {
        if (!currentUserService.UserId.HasValue)
        {
            return Result.Unauthorized();
        }

        var userId = currentUserService.UserId.Value;

        Guid teacherId = Guid.Empty;

        if (currentUserService.Role == AppRoles.Teacher)
        {
            teacherId = userId;
        }
        else if (currentUserService.Role == AppRoles.Assistant)
        {
            var assistantRepo = unitOfWork.GetOrCreateRepository<Assistant, Guid>();

            var teacherGuid = await assistantRepo.FirstOrDefaultAsync(
                new AssistantByIdWithProjectionSpec<Guid?>(userId, x => x.TeacherId),
                cancellationToken
            );
            if (!teacherGuid.HasValue)
            {
                return Result.Error("Assistant isn't assigned to a teacher");
            }

            teacherId = teacherGuid.Value;
        }
        else
        {
            return Result.Unauthorized();
        }

        var lessonRepository = unitOfWork.GetOrCreateRepository<Lesson, int>();

        var spec = new LessonByPublicIdWithMaterialsSpec(request.LessonId);

        var lesson = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (lesson is null)
        {
            return Result.NotFound($"Lesson with id '{request.LessonId}' was not found");
        }

        var objectStorageRepository = unitOfWork.GetOrCreateRepository<ObjectStorage, Guid>();

        var objectStorage = await objectStorageRepository.FirstOrDefaultAsync(
            new ObjectStorageByWorkspaceIdSpec(teacherId),
            cancellationToken
        );

        if (objectStorage is null || string.IsNullOrWhiteSpace(objectStorage.BucketName))
        {
            return Result.Error("An Error Happen, there's no space to upload to it");
        }

        var id = Guid.CreateVersion7();

        var objectKey = UrlsHelper.ConstructLessonMaterialsObjectKey(
            lessonPublicId: request.LessonId,
            objectId: id
        );

        var fileCreationResult = objectStorage.AddFile(
            fileId: id,
            objectKey: objectKey,
            fileName: request.FileName,
            contentType: request.ContentType,
            size: request.Size
        );

        if (!fileCreationResult.IsSuccess)
        {
            return fileCreationResult.Map();
        }

        var file = fileCreationResult.Value;

        // TODO : this should be handled(according to business logic and supported types)

        // var type =
        //     MimeTypeMapper.GetExtension(request.ContentType) == ".pdf"
        //         ? LessonMaterialType.PDF
        //         : LessonMaterialType.PPT;

        var result = lesson.AddMaterial(file.Id);
        if (!result.IsSuccess)
        {
            return result.Map();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var urlResult = await storageService.GetPreSignedUploadUrlAsync(
            objectStorage.BucketName,
            objectKey,
            1.DayInSeconds(),
            cancellationToken: cancellationToken
        );

        if (!urlResult.IsSuccess)
        {
            return urlResult.Map();
        }

        return new AddLessonMaterialCommandResponse(result.Value.Id, urlResult.Value);
    }

    private static class MimeTypeMapper
    {
        private static readonly Dictionary<string, string> MimeToExtensionMap = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            { "image/jpeg", ".jpg" },
            { "image/png", ".png" },
            { "image/gif", ".gif" },
            { "application/pdf", ".pdf" },
            { "application/json", ".json" },
            { "text/plain", ".txt" },
            { "text/html", ".html" },
            { "application/octet-stream", ".bin" },
            // Word
            { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx" },
            { "application/msword", ".doc" }, // Legacy Word
            // PowerPoint
            {
                "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".pptx"
            },
            { "application/vnd.ms-powerpoint", ".ppt" },
        };

        public static string GetExtension(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                return string.Empty;

            // Clean up parameters if present (e.g., "text/plain; charset=utf-8")
            var cleanMime = contentType.Split(';')[0].Trim();

            return MimeToExtensionMap.TryGetValue(cleanMime, out var extension)
                ? extension
                : string.Empty;
        }
    }
}
