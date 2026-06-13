
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.LessonCatalog.Queries;

public class GetLessonsCatalogQueryHandler : IRequestHandler<GetLessonsCatalogQuery,Result<ICollection<LessonCatalogDto>>>
{

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetLessonsCatalogQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }
    public async Task<Result<ICollection<LessonCatalogDto>>> Handle(GetLessonsCatalogQuery request, CancellationToken cancellationToken)
    {
        //if (_currentUser.UserId is null)
        //{
        //    return Result<ICollection<LessonCatalogDto>>
        //        .Failure("User not authenticated");
        //}

        var studentId = _currentUser.UserId.Value;

        var lessonRepo =
            _unitOfWork.GetOrCreateRepository<Lesson>();

        var lessons = await lessonRepo.ListAsync(
            new LessonsCatalogSpecification(), cancellationToken);

        var result = lessons
            .Select(x => MapLesson(x, studentId, lessons))
            .ToList();

        return Result<ICollection<LessonCatalogDto>>
            .Success(result);
    }

    private LessonCatalogDto MapLesson(Lesson lesson, Guid studentId, ICollection<Lesson> lessons)
    {
        var status =
            DetermineStatus(lesson,studentId,lessons);

        var enrollment = lesson.Enrollments
            .FirstOrDefault(x => x.StudentId == studentId);

        return new LessonCatalogDto
        {
            Id = lesson.Id,
            Title = lesson.Title,
            Price = lesson.Price,

            Status = status,

            ExpiredDate =
                status == LessonCatalogStatus.Expired
                    ? enrollment?.ExpiryDate
                    : null,

            PrerequisiteLabel =
                status == LessonCatalogStatus.Locked
                    ? "تحتاج لإكمال الدرس السابق"
                    : null
        };
    }

    private LessonCatalogStatus DetermineStatus(Lesson lesson, Guid studentId,ICollection<Lesson> allLessons)
    {
        var enrollment = lesson.Enrollments
            .FirstOrDefault(x => x.StudentId == studentId);

        if (enrollment is null)
        {
            return LessonCatalogStatus.Available;
        }
        //var expired = enrollment.ExpiresAt?? enrollment.EnrolledAt.AddDays(7);

        if (enrollment.ExpiryDate < DateTimeOffset.UtcNow)
        {
            return LessonCatalogStatus.Expired;
        }

        //if (lesson.PrerequisiteLessonId is not null)
        //{
        //    var prerequisiteLesson =
        //        allLessons.FirstOrDefault(
        //            x => x.Id ==
        //                 lesson.PrerequisiteLessonId);

        //    if (prerequisiteLesson is not null)
        //    {
        //        var prerequisitePurchased =
        //            prerequisiteLesson.Enrollments
        //                .Any(x => x.StudentId == studentId);

        //        if (prerequisitePurchased)
        //        {
        //            var completed =
        //                IsLessonCompleted(
        //                    prerequisiteLesson,
        //                    studentId);

        //            if (!completed)
        //            {
        //                return LessonCatalogStatus.Locked;
        //            }
        //        }
        //    }
        //}

        return LessonCatalogStatus.Purchased;
    }

    private static bool IsLessonCompleted(Lesson lesson, Guid studentId)
    {
        if (!lesson.Sections.Any())
        {
            return false;
        }

        return lesson.Sections.All(section =>
            section.Progresses.Any(progress =>
                progress.StudentId == studentId &&
                progress.IsCompleted));
    }
}
