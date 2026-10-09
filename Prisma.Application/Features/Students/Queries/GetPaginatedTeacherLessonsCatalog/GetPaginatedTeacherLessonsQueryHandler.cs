using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.DTOs;
using Prisma.Application.Features.Students.Queries.GetPaginatedLessonsCatalog;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Students.Queries.GetPaginatedTeacherLessonsCatalog;

internal sealed class GetPaginatedTeacherLessonsQueryHandler(
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork,
    IStorageService storageService
) : IRequestHandler<GetPaginatedTeacherLessonsQuery, Result<PaginatedList<LessonCatalogDto>>>
{
    public async Task<Result<PaginatedList<LessonCatalogDto>>> Handle(
        GetPaginatedTeacherLessonsQuery request,
        CancellationToken cancellationToken
    )
    {
        if (currentUser.UserId is null)
        {
            return Result.Unauthorized();
        }

        var studentId = currentUser.UserId.Value;

        var studentRepo = unitOfWork.GetOrCreateRepository<Student, Guid>();
        var student = await studentRepo.GetByIdAsync(studentId, cancellationToken);

        if (student is null)
        {
            return Result.Unauthorized();
        }

        if (student.AcademicYearId is null)
        {
            return Result.Error($"Student {studentId} has no academic year assigned.");
        }

        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();

        //var spec = new PaginatedEnrollmentPerTeacherSpecification(
        //    request.TeacherId,
        //    studentId,
        //    request.Keyword,
        //    request.Pagination.PageNumber,
        //    request.Pagination.PageSize
        //);
        var spec = new PagedTeacherLessonsCatalogSpecification(
            request.TeacherId,
            request.Keyword,
            request.Pagination.PageNumber,
            request.Pagination.PageSize,
            student.AcademicYearId.Value
        );
        var lessons = await lessonRepo.ListAsync(spec, cancellationToken);

        var lessonCount = await lessonRepo.CountAsync(spec, cancellationToken);
        // var result = lessons
        //     .Select(lesson => await MapLesson(lesson, studentId, lessons))
        //     .ToList();

        var result = (
            await Task.WhenAll(lessons.Select(lesson => MapLesson(lesson, student, lessons)))
        )
            .OfType<LessonCatalogDto>()
            .ToList();

        return new PaginatedList<LessonCatalogDto>()
        {
            Items = [.. result],
            PageNumber = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize,
            TotalCount = lessonCount,
        };
    }

    // private LessonCatalogDto MapLesson(Lesson lesson, Guid studentId, ICollection<Lesson> allLessons)

    private async Task<LessonCatalogDto> MapLesson(
        Lesson lesson,
        Student student,
        ICollection<Lesson> allLessons
    )
    {
        var studentId = student.Id;

        var status = DetermineStatus(lesson, studentId, allLessons);

        var enrollment = lesson.Enrollments.FirstOrDefault(x => x.StudentId == studentId);

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
            Title = lesson.Title,
            Money = status == EnrollmentStatus.Available ? lesson?.Money?.ToDto() : null,
            Status = status,
            PrerequisiteLabel = prerequisiteLabel,
            IsExpired = enrollment?.Status == EnrollmentStatus.Expired,
            ExpiresAt = enrollment?.ExpiresAt,
            TeacherName = teacherName,
            Subject = lesson?.Teacher?.Subject,
            Duration = new TimeDurationDto((int)(lesson?.Duration.TotalSeconds ?? 0)),
            ImageThumbnailUrl = imageThumbnailUrl,
        };
    }

    private static EnrollmentStatus DetermineStatus(
        Lesson lesson,
        Guid studentId,
        ICollection<Lesson> allLessons
    )
    {
        var enrollment = lesson.Enrollments.FirstOrDefault(x => x.StudentId == studentId);

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
