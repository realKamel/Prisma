using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonDetails;

public class GetLessonDetailsQueryHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService)
    : IRequestHandler<GetLessonDetailsQuery, Result<LessonDetailsDto>>
{
    public async Task<Result<LessonDetailsDto>> Handle(GetLessonDetailsQuery request,
        CancellationToken cancellationToken)
    {
        Guid? currentStudentId = _currentUserService.UserId;
        if(currentStudentId is null)
            throw new UnauthorizedException("User is not authenticated");

        var lessonrepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        var spec = new LessonWithDetailsSpecification(request.LessonId);
        var lesson = await lessonrepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (lesson == null)
        {
            throw new NotFoundException("Lesson", request.LessonId.ToString());
        }

        int totalMinutes = lesson.Sections != null ? (int)lesson.Sections.Sum(s => s.Duration.TotalMinutes) : 0;
        string formattedTotalDuration = FormatMinutesToHours(totalMinutes);

        bool isPrerequisiteCompleted = true; 

        if (lesson.Prerequisite != null)
        {
            var prereqLesson = await lessonrepository.FirstOrDefaultAsync(new LessonWithDetailsSpecification(lesson.Prerequisite.Id), cancellationToken);

            var enrollment = prereqLesson?.Enrollments?
                .FirstOrDefault(e => e.StudentId == currentStudentId && e.Status == EnrollmentStatus.Active);

            isPrerequisiteCompleted = enrollment?.IsCompleted == true;
        }

        var lessonDto = new LessonDetailsDto
        {
            Id = lesson.Id,
            Title = lesson.Title ?? "",
            Price = lesson.Price,
            AboutText = lesson.Description ?? "",
            StudentsCount = lesson.Enrollments?.Count ?? 0,
            ChaptersCount = lesson.Sections?.Count ?? 0,
            Subject = "لغه انجليزيه",
            Teacher = "أ. أحمد مصطفى",
            Duration = formattedTotalDuration,
            ValidityDays = 7,
            Chapters = lesson.Sections?.Select(s => new ChapterDto(
                s.Id,
                s.Title ?? "",
                $"{(int)s.Duration.TotalMinutes} د",
                s.IsPreview
            )).ToList() ?? [],
            Outcomes = lesson.Outcomes?.ToList() ?? [],
            Prerequisites = lesson.Prerequisite != null
                ? [new PrerequisiteDto(lesson.Prerequisite.Title ?? "", isPrerequisiteCompleted)]
                : []
        };

        return Result<LessonDetailsDto>.Success(lessonDto);
    }

    private string FormatMinutesToHours(int totalMinutes)
    {
        if (totalMinutes <= 0) return "٠ دقيقة";

        int hours = totalMinutes / 60;
        int remainingMinutes = totalMinutes % 60;

        if (hours > 0 && remainingMinutes > 0)
            return $"{hours} ساعة و {remainingMinutes} دقيقة";

        if (hours > 0)
            return $"{hours} ساعة";

        return $"{remainingMinutes} دقيقة";
    }
}