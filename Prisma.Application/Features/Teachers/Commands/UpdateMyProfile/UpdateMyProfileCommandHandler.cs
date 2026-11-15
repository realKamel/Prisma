using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Abstractions.ObjectStorage;
using Prisma.Application.Features.Teachers.Profile;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;
using Prisma.Domain.ValueObjects.UserDomain;

namespace Prisma.Application.Features.Teachers.Commands.UpdateMyProfile;

internal sealed class UpdateMyProfileCommandHandler(
    ICurrentUserService currentUserService,
    IStorageService storageService,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateMyProfileCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        UpdateMyProfileCommand request,
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

        var visibility = new ProfileVisibility
        {
            ShowYearsOfExperience = request.Visibility.ShowYearsOfExperience,
            ShowContactLinks = request.Visibility.ShowContactLinks,
            ShowStudentsCount = request.Visibility.ShowStudentsCount,
            ShowLessonsCount = request.Visibility.ShowLessonsCount,
            ShowPopularLessons = request.Visibility.ShowPopularLessons,
        };

        var updateResult = teacher.UpdateProfile(
            request.FirstName,
            request.SecondName,
            request.ThirdName,
            request.LastName,
            request.Subject.Trim(),
            string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim(),
            request.YearsOfExperience,
            request.Color,
            ContactLinkNormalizer.Normalize(request.Links),
            visibility
        );

        if (!updateResult.IsSuccess)
        {
            return updateResult;
        }

        teacher.SetAnnouncement(request.AnnouncementText, request.AnnouncementEndsAt);

        var removedFile = request.RemovePicture ? teacher.ProfileImageFile : null;
        var removedBucket = removedFile?.ObjectStorage.BucketName;
        var removedKey = removedFile?.ObjectKey;

        if (request.RemovePicture)
        {
            teacher.RemoveProfileImage();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (removedBucket is not null && removedKey is not null)
        {
            await DeleteRemovedObjectAsync(removedBucket, removedKey, cancellationToken);
        }

        return Result.Success(true);
    }

    /// <summary>
    /// Removes the picture's object from the bucket once the profile is saved. Best effort:
    /// a leftover object is harmless and must not fail the save.
    /// </summary>
    private async Task DeleteRemovedObjectAsync(
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