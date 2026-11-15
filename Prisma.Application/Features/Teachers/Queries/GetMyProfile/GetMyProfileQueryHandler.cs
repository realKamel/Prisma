using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Abstractions.ObjectStorage;
using Prisma.Application.Features.Teachers.Profile;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.Teachers;

namespace Prisma.Application.Features.Teachers.Queries.GetMyProfile;

internal sealed class GetMyProfileQueryHandler(
    ICurrentUserService currentUserService,
    IStorageService storageService,
    IUnitOfWork unitOfWork
) : IQueryHandler<GetMyProfileQuery, Result<MyProfileResponse>>
{
    private const int PictureLinkLifetimeInMinutes = 24 * 60;
    private const long MaxPictureSizeBytes = 10L * 1024 * 1024;

    private static readonly string[] AllowedPictureTypes = ["image/png", "image/webp", "image/jpeg"];

    public async Task<Result<MyProfileResponse>> Handle(
        GetMyProfileQuery request,
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

        var picture = await BuildPictureAsync(teacher, cancellationToken);

        var studentsCount = await unitOfWork
            .GetOrCreateRepository<TeacherStudent, int>()
            .CountAsync(new ActiveStudentsOfTeacherSpec(teacherId), cancellationToken);

        var lessonsCount = await unitOfWork
            .GetOrCreateRepository<Lesson, int>()
            .CountAsync(new ActiveLessonsOfTeacherSpec(teacherId), cancellationToken);

        var links = teacher.Links;
        var visibility = teacher.Visibility;

        return new MyProfileResponse(
            FirstName: teacher.FirstName,
            SecondName: teacher.SecondName,
            ThirdName: teacher.ThirdName ?? string.Empty,
            LastName: teacher.LastName ?? string.Empty,
            Subject: teacher.Subject,
            Bio: teacher.Bio,
            YearsOfExperience: teacher.YearsOfExperience,
            Color: teacher.Color,
            Links: new ContactLinksDto(
                links.Whatsapp,
                links.Facebook,
                links.Instagram,
                links.Youtube,
                links.Telegram,
                links.Tiktok,
                links.Website
            ),
            Visibility: new ProfileVisibilityDto(
                visibility.ShowYearsOfExperience,
                visibility.ShowContactLinks,
                visibility.ShowStudentsCount,
                visibility.ShowLessonsCount,
                visibility.ShowPopularLessons
            ),
            Picture: picture,
            // Popular lessons are left empty for now.
            Stats: new ProfileStatsDto(studentsCount, lessonsCount, [])
        );
    }

    /// <summary>
    /// The browser PUTs the picture straight to storage, so the object being there is the proof the
    /// upload finished. No object (failed or unfinished PUT): no picture. The object must also be
    /// a real image of an allowed type and size, since nothing scans these files.
    /// </summary>
    private async Task<ProfilePictureDto> BuildPictureAsync(
        Teacher teacher,
        CancellationToken cancellationToken
    )
    {
        var file = teacher.ProfileImageFile;
        if (file is null)
        {
            return new ProfilePictureDto(null);
        }

        var bucketName = file.ObjectStorage.BucketName;

        if (!await IsUploadedImageAsync(bucketName, file.ObjectKey, cancellationToken))
        {
            return new ProfilePictureDto(null);
        }

        var url = await storageService.GetDownloadUrlAsync(
            bucketName,
            file.ObjectKey,
            PictureLinkLifetimeInMinutes
        );

        return new ProfilePictureDto(url);
    }

    private async Task<bool> IsUploadedImageAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await storageService.GetObjectMetadataAsync(
                bucketName,
                objectKey,
                cancellationToken
            );

            return result.IsSuccess
                && result.Value.SizeInBytes is > 0 and <= MaxPictureSizeBytes
                && AllowedPictureTypes.Contains(
                    result.Value.ContentType,
                    StringComparer.OrdinalIgnoreCase
                );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // S3 throws when the object is missing; R2 returns NotFound. Same answer for both.
            return false;
        }
    }
}