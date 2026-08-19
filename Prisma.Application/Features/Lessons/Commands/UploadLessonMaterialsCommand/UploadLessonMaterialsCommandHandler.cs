using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Commands.UploadLessonMaterialsCommand;

public class UploadLessonMaterialsCommandHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    IIdentityService identityService,
    IStorageService storageService
) : IRequestHandler<UploadLessonMaterialsCommand, Result>
{
    public async Task<Result> Handle(
        UploadLessonMaterialsCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            return Result.Unauthorized("User must be authenticated.");

        var user = await identityService.FindByIdAsync(userId.Value, cancellationToken);

        if (user is null)
            return Result.Unauthorized("User not found.");

        var roles = await identityService.GetRolesAsync(user);
        if (
            !roles.Contains(AppRoles.Teacher)
            && !roles.Contains(AppRoles.Assistant)
            && !roles.Contains(AppRoles.Admin)
        )
            return Result.Unauthorized(
                "Only teachers and assistants can upload materials to lessons."
            );

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();

        var spec = new LessonWithMaterialsForUpdateSpecification(request.LessonId);

        var lesson = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (lesson is null)
            return Result.NotFound($"Lesson with id '{request.LessonId}' was not found");

        if (request.Files == null || !request.Files.Any())
            return Result.Error("No files provided for upload.");

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

                lesson.LessonMaterials.Add(
                    new LessonMaterial
                    {
                        Title = Path.GetFileNameWithoutExtension(file.FileName),
                        Size = fileSize,
                        Type = materialType,
                        DownloadUrl = filename,
                        LessonId = lesson.Id,
                    }
                );
            }
        }

        lessonRepository.Update(lesson);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessWithMessage("Materials uploaded and attached to lesson successfully");
    }
}
