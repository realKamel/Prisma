using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.DTOs;
using Prisma.Application.Features.Students.Queries.GetPaginatedLessonsCatalog;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Students.Queries.Helpers;

internal static class LessonMappingHelper
{
    public static async Task<LessonCatalogDto?> MapLesson(
        IStorageService storageService,
        Enrollment? enrollment,
        Student? student
    )
    {
        if (student is null || enrollment is null)
        {
            return null;
        }

        var studentId = student.Id;

        var status = DetermineStatus(enrollment, studentId);

        var lesson = enrollment.Lesson;

        if (lesson is null)
        {
            return null;
        }

        var teacherName =
            lesson.Teacher?.FullName ?? $"{lesson.Teacher?.FirstName} {lesson.Teacher?.LastName}";

        var imageThumbnailUrl =
            lesson.ImageThumbnailUrl != null
                ? await storageService.GetDownloadUrlAsync(
                    storageService.DefaultBucketName,
                    lesson.ImageThumbnailUrl
                )
                : string.Empty;

        var prerequisiteLabel =
            status == EnrollmentStatus.Locked ? "تحتاج لإكمال الدرس السابق" : null;

        return new LessonCatalogDto
        {
            Id = lesson.Id,
            PublicId = lesson.PublicId,
            Title = lesson.Title,
            Money = status == EnrollmentStatus.Available ? enrollment.Payment?.Money.ToDto() : null,
            Status = status,
            IsExpired = enrollment.Status == EnrollmentStatus.Expired,
            PrerequisiteLabel = prerequisiteLabel,
            ExpiresAt = enrollment.ExpiresAt,
            TeacherName = teacherName,
            Subject = lesson.Teacher?.Subject,
            Duration = new TimeDurationDto((int)lesson.Duration.TotalSeconds),
            ImageThumbnailUrl = imageThumbnailUrl,
        };
    }

    private static EnrollmentStatus DetermineStatus(Enrollment? enrollment, Guid studentId)
    {
        if (enrollment is null)
        {
            return EnrollmentStatus.Available;
        }

        if (enrollment.ExpiresAt.HasValue && enrollment.ExpiresAt.Value < DateTimeOffset.UtcNow)
        {
            return EnrollmentStatus.Expired;
        }

        if (enrollment.Lesson?.PrerequisiteId is not null)
        {
            var prerequisiteLesson = enrollment.Lesson.Prerequisite;

            if (prerequisiteLesson is not null)
            {
                var prerequisiteEnrollment = prerequisiteLesson.Enrollments.FirstOrDefault(x =>
                    x.StudentId == studentId
                );

                // Student bought the prerequisite lesson
                if (prerequisiteEnrollment is not null)
                {
                    if (!prerequisiteEnrollment.IsCompleted)
                    {
                        return EnrollmentStatus.Locked;
                    }
                }
            }
        }

        return EnrollmentStatus.Active; // Previously LessonCatalogStatus.Purchased
    }
}
