using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.LessonCatalog.Queries;

public class GetLessonsCatalogQueryHandler
    : IRequestHandler<GetLessonsCatalogQuery, Result<ICollection<LessonCatalogDto>>>
{
    private readonly IUnitOfWork          _unitOfWork;
    private readonly ICurrentUserService  _currentUser;

    public GetLessonsCatalogQueryHandler(
        IUnitOfWork         unitOfWork,
        ICurrentUserService currentUser)
    {
        _unitOfWork  = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<ICollection<LessonCatalogDto>>> Handle(
        GetLessonsCatalogQuery request,
        CancellationToken      cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Result<ICollection<LessonCatalogDto>>
                .Failure("User not authenticated");

        var studentId = _currentUser.UserId.Value;

        var lessonRepo = _unitOfWork.GetOrCreateRepository<Lesson>();

        var lessons = await lessonRepo.ListAsync(
            new LessonsCatalogSpecification(), cancellationToken);

        var result = lessons
            .Select(lesson => MapLesson(lesson, studentId, lessons))
            .ToList();

        return Result<ICollection<LessonCatalogDto>>.Success(result);
    }

    private LessonCatalogDto MapLesson(
        Lesson             lesson,
        Guid               studentId,
        ICollection<Lesson> allLessons)
    {
        var status = DetermineStatus(lesson, studentId, allLessons);

        var enrollment = lesson.Enrollments
            .FirstOrDefault(x => x.StudentId == studentId);

        var statusString = status switch
        {
            LessonCatalogStatus.Available => "avail",
            LessonCatalogStatus.Purchased => "purchased",
            LessonCatalogStatus.Locked    => "locked",
            LessonCatalogStatus.Expired   => "expired",
            _                             => "avail"
        };

        string? expiredDateLabel = null;
        if (status == LessonCatalogStatus.Expired && enrollment?.ExpiryDate is not null)
        {
            var d = enrollment.ExpiryDate.Value;
            expiredDateLabel =
                $"انتهت صلاحيتك · انتهت في {ToArabicNumerals(d.Day.ToString())} {GetArabicMonth(d.Month)}";
        }

        return new LessonCatalogDto
        {
            Id            = lesson.Id,
            Title         = lesson.Title,
            Price         = status == LessonCatalogStatus.Available ? lesson.Price : 0,
            Status        = statusString,
            PrerequisiteLabel = status == LessonCatalogStatus.Locked
                ? "تحتاج لإكمال الدرس السابق"
                : null,
            ExpiredDate   = expiredDateLabel,
            TeacherName    = "أ. أحمد مصطفى",
            TeacherInitial = "أ",
            Subject        = "فيزياء",
            DurationHours  = lesson.Sections?.Count ?? 0,
            Currency       = "جنيه",
        };
    }

    private LessonCatalogStatus DetermineStatus(
        Lesson              lesson,
        Guid                studentId,
        ICollection<Lesson> allLessons)
    {
        var enrollment = lesson.Enrollments
            .FirstOrDefault(x => x.StudentId == studentId);

        if (enrollment is null)
            return LessonCatalogStatus.Available;

        if (enrollment.ExpiryDate is not null &&
            enrollment.ExpiryDate.Value < DateTimeOffset.UtcNow)
            return LessonCatalogStatus.Expired;

        // 3. Prerequisite check — uncomment when PrerequisiteLessonId is wired up
        // if (lesson.PrerequisiteLessonId is not null)
        // {
        //     var pre = allLessons.FirstOrDefault(x => x.Id == lesson.PrerequisiteLessonId);
        //     if (pre is not null)
        //     {
        //         var prePurchased = pre.Enrollments.Any(x => x.StudentId == studentId);
        //         if (prePurchased && !IsLessonCompleted(pre, studentId))
        //             return LessonCatalogStatus.Locked;
        //     }
        // }

        return LessonCatalogStatus.Purchased;
    }

    private static bool IsLessonCompleted(Lesson lesson, Guid studentId)
    {
        if (!lesson.Sections.Any()) return false;

        return lesson.Sections.All(section =>
            section.Progresses.Any(p =>
                p.StudentId == studentId && p.IsCompleted));
    }

    private static string GetArabicMonth(int month) => month switch
    {
        1  => "يناير",
        2  => "فبراير",
        3  => "مارس",
        4  => "أبريل",
        5  => "مايو",
        6  => "يونيو",
        7  => "يوليو",
        8  => "أغسطس",
        9  => "سبتمبر",
        10 => "أكتوبر",
        11 => "نوفمبر",
        12 => "ديسمبر",
        _  => ""
    };

    private static string ToArabicNumerals(string str) =>
        str.Replace("0", "٠").Replace("1", "١").Replace("2", "٢") .Replace("3", "٣").Replace("4", "٤").Replace("5", "٥") .Replace("6", "٦").Replace("7", "٧").Replace("8", "٨") .Replace("9", "٩");
}