using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Commands.DeleteLessonMaterialCommand;

public class DeleteLessonMaterialCommandHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    UserManager<User> _userManager,
    IStorageService storageService
) : IRequestHandler<DeleteLessonMaterialCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteLessonMaterialCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            return Result.Unauthorized("User must be authenticated.");
        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null)
            return Result.Unauthorized("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(AppRoles.Teacher) && !roles.Contains(AppRoles.Assistant) && !roles.Contains(AppRoles.Admin))
            return Result.Unauthorized("Only teachers and assistants can delete materials from lessons.");

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        var spec = new LessonWithMaterialsForUpdateSpecification(request.LessonId); var lesson = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (lesson is null)
            return Result.NotFound($"Lesson material with id '{request.MaterialId}' was not found");

        var materialToDelete = lesson.LessonMaterials.FirstOrDefault(m => m.Id == request.MaterialId);
        if (materialToDelete != null)
        {
            await storageService.DeleteFileAsync(storageService.DefaultBucketName, materialToDelete.DownloadUrl, cancellationToken);

            lesson.LessonMaterials.Remove(materialToDelete);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<string>.Success("Material deleted successfully.");
    }
}