using Ardalis.Result;
using Ardalis.Specification;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Abstractions.ObjectStorage;
using Prisma.Application.Features.Students.Queries.GetTeacherCatalog;
using Prisma.Application.Features.Teachers.Profile;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.ObjectLibraryAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.ObjectStorageSpec;
using Prisma.Domain.Specifications.Teachers;
using Prisma.Domain.ValueObjects.UserDomain;

namespace Prisma.Application.Features.Teachers.Queries.GetPublicTeacherProfile;

internal sealed class GetPublicTeacherProfileQueryHandler(
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IStorageService storageService
) : IQueryHandler<GetPublicTeacherProfileQuery, Result<PublicTeacherProfileResponse>>
{
    private const int FeaturedLessonsCount = 3;

    // Same rules as GetMyProfile.
    private const int PictureLinkLifetimeInMinutes = 24 * 60;
    private const long MaxPictureSizeBytes = 10L * 1024 * 1024;

    private static readonly string[] AllowedPictureTypes = ["image/png", "image/webp", "image/jpeg"];

    public async Task<Result<PublicTeacherProfileResponse>> Handle(
        GetPublicTeacherProfileQuery request,
        CancellationToken cancellationToken
    )
    {
        var teacherId = request.Id;

        var teacher = await unitOfWork
            .GetOrCreateRepository<Teacher, Guid>()
            .FirstOrDefaultAsync(new TeacherProfileSpec(teacherId), cancellationToken);

        if (teacher is null)
        {
            return Result.NotFound();
        }

        var visibility = teacher.Visibility;

        // Same specs as GetMyProfile, so the public numbers match what the teacher sees.
        // Only queried when the teacher chose to show them.
        int? studentsCount = null;
        if (visibility.ShowStudentsCount)
        {
            studentsCount = await unitOfWork
                .GetOrCreateRepository<TeacherStudent, int>()
                .CountAsync(new ActiveStudentsOfTeacherSpec(teacherId), cancellationToken);
        }

        int? lessonsCount = null;
        if (visibility.ShowLessonsCount)
        {
            lessonsCount = await unitOfWork
                .GetOrCreateRepository<Lesson, int>()
                .CountAsync(new ActiveLessonsOfTeacherSpec(teacherId), cancellationToken);
        }

        // Lessons are only for signed-in visitors; anonymous requests get an empty list
        // (and the query is skipped), so the page hides the section.
        IReadOnlyList<ProfileLessonDto> lessons = [];
        if (visibility.ShowPopularLessons && currentUserService.UserId is not null)
        {
            lessons = await unitOfWork
                .GetOrCreateRepository<Lesson, int>()
                .ListAsync(
                    new LatestLessonsOfTeacherSpec(teacherId, FeaturedLessonsCount),
                    cancellationToken
                );

            lessons = await WithThumbnailLinksAsync(teacherId, lessons, cancellationToken);
        }

        return new PublicTeacherProfileResponse(
            teacher.Id,
            teacher.FirstName,
            teacher.SecondName,
            teacher.Subject,
            string.IsNullOrWhiteSpace(teacher.Bio) ? null : teacher.Bio,
            await BuildPictureUrlAsync(
                teacher.PictureBucketName,
                teacher.PictureObjectKey,
                cancellationToken
            ),
            teacher.Color,
            lessonsCount,
            studentsCount,
            visibility.ShowYearsOfExperience ? teacher.YearsOfExperience : null,
            BuildLinks(teacher.Links, visibility.ShowContactLinks),
            teacher.AcademicYears,
            Lessons: lessons,
            Announcement: BuildAnnouncement(teacher.AnnouncementText, teacher.AnnouncementEndsAt),
            CanMessage: currentUserService.UserId is { } viewerId && viewerId != teacherId,
            IsSignedIn: currentUserService.UserId is not null,
            Featured: teacher.IsFeatured
        );
    }

    /// <summary>
    /// Same flow as GetMyProfile: the browser PUTs the picture straight to storage, so the object
    /// being there is the proof the upload finished. No object, or not a real image of an allowed
    /// type and size: no picture.
    /// </summary>
    private async Task<string?> BuildPictureUrlAsync(
        string? bucketName,
        string? objectKey,
        CancellationToken cancellationToken
    )
    {
        if (bucketName is null || objectKey is null)
        {
            return null;
        }

        if (!await IsUploadedImageAsync(bucketName, objectKey, cancellationToken))
        {
            return null;
        }

        return await storageService.GetDownloadUrlAsync(
            bucketName,
            objectKey,
            PictureLinkLifetimeInMinutes
        );
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

    /// <summary>
    /// Lessons store the cover's object key, and covers live in the teacher's object storage.
    /// Swaps each key for a download link (local signing, no network call per lesson).
    /// A lesson whose key can't be resolved ends up with no cover, so the card shows its fallback.
    /// </summary>
    private async Task<IReadOnlyList<ProfileLessonDto>> WithThumbnailLinksAsync(
        Guid teacherId,
        IReadOnlyList<ProfileLessonDto> lessons,
        CancellationToken cancellationToken
    )
    {
        if (lessons.All(l => string.IsNullOrWhiteSpace(l.ImageThumbnailUrl)))
        {
            return lessons;
        }

        var storage = await unitOfWork
            .GetOrCreateRepository<ObjectStorage, Guid>()
            .FirstOrDefaultAsync(new ObjectStorageByTeacherId(teacherId), cancellationToken);

        var result = new List<ProfileLessonDto>(lessons.Count);
        foreach (var lesson in lessons)
        {
            var key = lesson.ImageThumbnailUrl;

            if (string.IsNullOrWhiteSpace(key) || storage is null)
            {
                result.Add(lesson with { ImageThumbnailUrl = null });
                continue;
            }

            var url = await storageService.GetDownloadUrlAsync(
                storage.BucketName,
                key,
                PictureLinkLifetimeInMinutes
            );

            result.Add(lesson with { ImageThumbnailUrl = url });
        }

        return result;
    }

    private static AnnouncementDto? BuildAnnouncement(string? text, DateTimeOffset? endsAt)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (endsAt is { } end && end <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        return new AnnouncementDto(text, endsAt);
    }

    private static ContactLinksDto? BuildLinks(ContactLinks links, bool isVisible)
    {
        if (!isVisible)
        {
            return null;
        }

        string?[] all =
        [
            links.Whatsapp,
            links.Facebook,
            links.Instagram,
            links.Youtube,
            links.Telegram,
            links.Tiktok,
            links.Website,
        ];

        if (all.All(string.IsNullOrWhiteSpace))
        {
            return null;
        }

        return new ContactLinksDto(
            links.Whatsapp,
            links.Facebook,
            links.Instagram,
            links.Youtube,
            links.Telegram,
            links.Tiktok,
            links.Website
        );
    }

    /// <summary>The teacher columns the public page needs, and nothing else.</summary>
    private sealed record TeacherProfileData(
        Guid Id,
        string FirstName,
        string SecondName,
        string Subject,
        string? Bio,
        ProfileColor Color,
        int? YearsOfExperience,
        bool IsFeatured,
        ContactLinks Links,
        ProfileVisibility Visibility,
        IReadOnlyList<AcademicYearDto> AcademicYears,
        string? PictureBucketName,
        string? PictureObjectKey,
        string? AnnouncementText,
        DateTimeOffset? AnnouncementEndsAt
    );

    private sealed class TeacherProfileSpec : Specification<Teacher, TeacherProfileData>
    {
        public TeacherProfileSpec(Guid teacherId)
        {
            Query.AsNoTracking().Where(t => t.Id == teacherId);

            Query.Select(t => new TeacherProfileData(
                t.Id,
                t.FirstName,
                t.SecondName,
                t.Subject,
                t.Bio,
                t.Color,
                t.YearsOfExperience,
                t.IsFeatured,
                t.Links,
                t.Visibility,
                t.AcademicYears
                    .Select(ay => new AcademicYearDto(ay.Id.ToString(), ay.AcademicYear.Title))
                    .ToList(),
                t.ProfileImageFile == null
                    ? null
                    : t.ProfileImageFile.ObjectStorage.BucketName,
                t.ProfileImageFile == null ? null : t.ProfileImageFile.ObjectKey,
                t.AnnouncementText,
                t.AnnouncementEndsAt
            ));
        }
    }

    /// <summary>
    /// The teacher's newest live lessons, projected straight into the card DTO
    /// (one query, no entities loaded). Newest = highest id.
    /// </summary>
    private sealed class LatestLessonsOfTeacherSpec : Specification<Lesson, ProfileLessonDto>
    {
        public LatestLessonsOfTeacherSpec(Guid teacherId, int take)
        {
            Query
                .AsNoTracking()
                .Where(l => l.TeacherId == teacherId && l.Status == LessonStatus.Active)
                .OrderByDescending(l => l.Id)
                .Take(take);

            Query.Select(l => new ProfileLessonDto(
                l.PublicId,
                l.Title ?? string.Empty,
                l.TimeDuration,
                l.Money,
                l.ImageThumbnailUrl,
                l.Enrollments.Count,
                l.Sections.Count
            ));
        }
    }
}