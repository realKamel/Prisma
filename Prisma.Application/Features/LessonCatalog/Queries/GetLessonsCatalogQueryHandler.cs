using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.LessonCatalog.Queries;

public class GetLessonsCatalogQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser)
    : IRequestHandler<GetLessonsCatalogQuery, Result<ICollection<LessonCatalogDto>>>
{
    public async Task<Result<ICollection<LessonCatalogDto>>> Handle(GetLessonsCatalogQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
            throw new UnauthorizedException();

        var studentId = currentUser.UserId.Value;

        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();

        var lessons = await lessonRepo.ListAsync(
            new LessonsCatalogSpecification(), cancellationToken);

        var result = lessons
            .Select(lesson => MapLesson(lesson, studentId, lessons))
            .ToList();

        return Result<ICollection<LessonCatalogDto>>.Success(result);
    }

    private LessonCatalogDto MapLesson(Lesson lesson, Guid studentId, ICollection<Lesson> allLessons)
    {
        var status = DetermineStatus(lesson, studentId, allLessons);

        var enrollment = lesson.Enrollments
            .FirstOrDefault(x => x.StudentId == studentId);

        var statusString = status switch
        {
            LessonCatalogStatus.Available => "avail",
            LessonCatalogStatus.Purchased => "purchased",
            LessonCatalogStatus.Locked => "locked",
            LessonCatalogStatus.Expired => "expired",
            _ => "avail"
        };

        string? expiredDateLabel = null;
        if (status == LessonCatalogStatus.Expired && enrollment?.ExpiresAt is not null)
        {
            var d = enrollment.ExpiresAt.Value;
            expiredDateLabel =
                $"انتهت صلاحيتك · انتهت في {ToArabicNumerals(d.Day.ToString())} {GetArabicMonth(d.Month)}";
        }

        return new LessonCatalogDto
        {
            Id = lesson.Id,
            Title = lesson.Title,
            Price = status == LessonCatalogStatus.Available ? lesson.Price : 0,
            Status = statusString,
            PrerequisiteLabel = status == LessonCatalogStatus.Locked
                ? "تحتاج لإكمال الدرس السابق"
                : null,
            ExpiredDate = expiredDateLabel,
            TeacherName = "أ. أحمد مصطفى",
            TeacherInitial = "أ",
            Subject = "اللغة الإنجليزية",
            DurationHours = (int)lesson.Duration.TotalHours,
            ImageThumbnailUrl = lesson.ImageThumbnailUrl,
            Currency = "جنيه",
        };
    }

    private LessonCatalogStatus DetermineStatus(
        Lesson lesson,
        Guid studentId,
        ICollection<Lesson> allLessons)
    {
        var enrollment = lesson.Enrollments
            .FirstOrDefault(x => x.StudentId == studentId);

        if (enrollment is null)
            return LessonCatalogStatus.Available;

        if (enrollment.ExpiresAt.HasValue &&
            enrollment.ExpiresAt.Value < DateTimeOffset.UtcNow)
            return LessonCatalogStatus.Expired;

        if (lesson.PrerequisiteId is not null)
        {
            var prerequisiteLesson = allLessons
                .FirstOrDefault(x => x.Id == lesson.PrerequisiteId);

            if (prerequisiteLesson is not null)
            {
                var prerequisiteEnrollment =
                    prerequisiteLesson.Enrollments
                        .FirstOrDefault(x =>
                            x.StudentId == studentId);

                // student buy the prerequisite Lesson 
                if (prerequisiteEnrollment is not null)
                {
                    if (!prerequisiteEnrollment.IsCompleted)
                    {
                        return LessonCatalogStatus.Locked;
                    }
                }
            }
        }

        return LessonCatalogStatus.Purchased;
    }


    private static string GetArabicMonth(int month) => month switch
    {
        1 => "يناير",
        2 => "فبراير",
        3 => "مارس",
        4 => "أبريل",
        5 => "مايو",
        6 => "يونيو",
        7 => "يوليو",
        8 => "أغسطس",
        9 => "سبتمبر",
        10 => "أكتوبر",
        11 => "نوفمبر",
        12 => "ديسمبر",
        _ => ""
    };

    private static string ToArabicNumerals(string str) =>
        str.Replace("0", "٠").Replace("1", "١").Replace("2", "٢")
            .Replace("3", "٣").Replace("4", "٤").Replace("5", "٥")
            .Replace("6", "٦").Replace("7", "٧").Replace("8", "٨")
            .Replace("9", "٩");
}