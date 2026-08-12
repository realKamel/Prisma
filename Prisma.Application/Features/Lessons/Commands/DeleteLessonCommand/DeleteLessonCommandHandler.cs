using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Commands.DeleteLessonCommand;

public class DeleteLessonCommandHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    IIdentityService identityService,
    IStorageService storageService,
    IVideoStorageService videoStorageService)
    : IRequestHandler<DeleteLessonCommand, Result>
{
    public async Task<Result> Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Result.Unauthorized();
        }

        var user = await identityService.FindByIdAsync(userId.Value, cancellationToken);

        if (user is null || !user.Roles.Any(r => IsInRole(r.Role.Name)))
        {
            return Result.Unauthorized();
        }

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();

        var lesson = await lessonRepository.FirstOrDefaultAsync(new LessonWithEnrollmentSpec(request.LessonId), cancellationToken);

        if (lesson is null)
        {
            return Result.NotFound($"Lesson with id '{request.LessonId}' was not found");
        }

        if (lesson.Enrollments != null)
        {
            lesson.Enrollments.Clear();
        }

        if (lesson.ImageThumbnailUrl != null)
        {
            await storageService.DeleteFileAsync(storageService.DefaultBucketName, lesson.ImageThumbnailUrl, cancellationToken);
        }

        foreach (var section in lesson.Sections)
            if (section.AssetId != null)
                await videoStorageService.DeleteVideoAsync(section.AssetId, cancellationToken);

        lessonRepository.Delete(lesson);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }

    private static bool IsInRole(string? role)
    {
        if (role is null)
        {
            return false;
        }
        return role == AppRoles.Admin || role == AppRoles.Teacher || role == AppRoles.Assistant;
    }
}