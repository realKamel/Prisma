using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.EnrollmentAggregate;
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
        var enrollmentRepository = _unitOfWork.GetOrCreateRepository<Enrollment, int>();

        var lessonInfo = await lessonRepository.FirstOrDefaultAsync(
            new LessonWithProjectionSpec<LessonDeletionInfo>(request.LessonId, l => new LessonDeletionInfo(
                l.Id,
                l.ImageThumbnailUrl,
                l.Enrollments.Select(e => e.Id).ToList(),
                l.Sections.Select(s => s.AssetId).ToList()
            )),
            cancellationToken);

        if (lessonInfo is null)
        {
            return Result.NotFound($"Lesson with id '{request.LessonId}' was not found");
        }

        foreach (var enrollmentId in lessonInfo.EnrollmentIds)
        {
            var stub = new Enrollment { Id = enrollmentId };
            enrollmentRepository.Delete(stub);
        }

        if (lessonInfo.ImageThumbnailUrl != null)
        {
            await storageService.DeleteFileAsync(storageService.DefaultBucketName, lessonInfo.ImageThumbnailUrl, cancellationToken);
        }

        foreach (var assetId in lessonInfo.SectionAssetIds.Where(a => a != null))
            await videoStorageService.DeleteVideoAsync(assetId!, cancellationToken);

        var lessonStub = new Lesson { Id = lessonInfo.Id };
        lessonRepository.Delete(lessonStub);

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
    public sealed record LessonDeletionInfo(
        int Id,
        string? ImageThumbnailUrl,
        List<int> EnrollmentIds,
        List<string?> SectionAssetIds
    );
}