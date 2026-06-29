using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;

namespace Prisma.Application.Features.Lessons.Commands.UploadLessonMaterials;

public class UploadLessonMaterialsCommandHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    UserManager<User> _userManager,
    IStorageService storageService
) : IRequestHandler<UploadLessonMaterialsCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UploadLessonMaterialsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User must be authenticated.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new UnauthorizedException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(AppRoles.Teacher)&& !roles.Contains(AppRoles.Assistant))
            throw new UnauthorizedException("Only teachers and assistants can upload materials to lessons.");

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        var spec = new UploadLessonMaterialsSpecification(request.LessonId);

        var lesson = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (lesson is null)
            throw new NotFoundException("Lesson", request.LessonId);

        if (request.Files == null || !request.Files.Any())
            throw new BadRequestException("No files provided for upload.");

        foreach (var file in request.Files)
        {
            if (file.Length > 0)
            {
                var filename = $"material/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                using var stream = file.OpenReadStream();

                await storageService.UploadFileAsync("prisma",filename ,stream ,file.ContentType, cancellationToken);

                string fileSize = file.Length < 1024 * 1024
                    ? $"{Math.Round((double)file.Length / 1024, 1)} KB"
                    : $"{Math.Round((double)file.Length / (1024 * 1024), 1)} MB";

                var ext = Path.GetExtension(file.FileName).ToLower();
                var materialType = ext switch
                {
                    ".pdf" => LessonMaterialType.PDF,
                    ".ppt" or ".pptx" => LessonMaterialType.PPT,
                    _ => LessonMaterialType.PDF
                };

                lesson.LessonMaterials.Add(new LessonMaterial
                {
                    Title = Path.GetFileNameWithoutExtension(file.FileName),
                    Size = fileSize,
                    Type = materialType,
                    DownloadUrl = filename,
                    LessonId = lesson.Id
                });
            }
        }

        lessonRepository.Update(lesson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Materials uploaded and attached to lesson successfully");
    }
}