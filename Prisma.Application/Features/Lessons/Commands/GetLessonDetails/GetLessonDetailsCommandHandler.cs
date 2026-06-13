using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Lessons.Commands.GetLessonDetails;

public class GetLessonDetailsCommandHandler(IRepository<Lesson> _lessonRepository)
    : IRequestHandler<GetLessonDetailsCommand, Result<LessonDetailsDto>>
{
    public async Task<Result<LessonDetailsDto>> Handle(GetLessonDetailsCommand request, CancellationToken cancellationToken)
    {
        var spec = new LessonWithDetailsSpecification(request.LessonId);
        var lesson = await _lessonRepository.GetBySpecAsync(spec, cancellationToken, tracking: false);

        if (lesson == null)
        {
            throw new NotFoundException("Lesson", request.LessonId.ToString());
        }

        int totalMinutes = lesson.Sections?.Sum(s => s.DurationInMinutes ?? 0) ?? 0;
        string formattedTotalDuration = FormatMinutesToHours(totalMinutes);

        bool isPrerequisiteCompleted = false;

        if (lesson.Prerequisite != null && request.StudentId.HasValue)
        {
            var prereqSpec = new LessonWithDetailsSpecification(lesson.PrerequisiteId.Value);
            var prereqLesson = await _lessonRepository.GetBySpecAsync(prereqSpec, cancellationToken);

            if (prereqLesson != null)
            {
                bool isEnrolled = prereqLesson.Enrollments?
                    .Any(e => e.StudentId == request.StudentId && e.Status == EnrollmentStatus.Active) ?? false;

                if (isEnrolled)
                {
                    int totalPrereqSections = prereqLesson.Sections?.Count ?? 0;

                    int completedPrereqSections = prereqLesson.Sections?
                        .Count(s => s.Progresses != null &&
                                    s.Progresses.Any(p => p.StudentId == request.StudentId && p.IsCompleted)) ?? 0;

                    if (totalPrereqSections > 0 && completedPrereqSections == totalPrereqSections)
                    {
                        isPrerequisiteCompleted = true;
                    }
                }
            }
        }

        var lessonDto = new LessonDetailsDto
        {
            Id = lesson.Id,
            Title = lesson.Title ?? "",
            Price = lesson.Price,
            AboutText = lesson.Description ?? "",
            StudentsCount = lesson.Enrollments?.Count ?? 0,
            ChaptersCount = lesson.Sections?.Count ?? 0,

            Subject = "فيزياء",
            Teacher = "أ.أحمد مصطفى", 
            Duration = formattedTotalDuration,

            ValidityDays = lesson.EndDate.HasValue
                            ? (int)(lesson.EndDate.Value - DateTimeOffset.UtcNow).TotalDays
                            : 30,

            Chapters = lesson.Sections?.Select(s => new ChapterDto(
                s.Id,
                s.Title ?? "",
                s.DurationInMinutes.HasValue ? $"{s.DurationInMinutes} د" : "٠ د",
                s.IsPreview
            )).ToList() ?? [],

            Outcomes = lesson.Outcomes?.Select(o => o.Text).ToList() ?? [],

            Prerequisites = lesson.Prerequisite != null
                ? [new PrerequisiteDto(lesson.Prerequisite.Title ?? "", isPrerequisiteCompleted)]
                : []
        };

        return Result<LessonDetailsDto>.Success(lessonDto);
    }

    private string FormatMinutesToHours(int totalMinutes)
    {
        if (totalMinutes == 0) return "٠ دقيقة";
        int hours = totalMinutes / 60;
        int remainingMinutes = totalMinutes % 60;

        if (hours > 0 && remainingMinutes > 0)
            return $"{hours} ساعات";
        if (hours > 0)
            return $"{hours} ساعات";

        return $"{remainingMinutes} د";
    }
}
