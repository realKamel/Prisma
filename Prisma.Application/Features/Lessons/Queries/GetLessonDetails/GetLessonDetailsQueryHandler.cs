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

        var lessonrepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        var spec = new LessonWithDetailsSpecification(request.LessonId);
        var lesson = await lessonrepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (lesson == null)
        {
            throw new NotFoundException("Lesson", request.LessonId.ToString());
        }

        int totalMinutes = lesson.Sections != null ? (int)lesson.Sections.Sum(s => s.Duration.TotalMinutes) : 0;
        string formattedTotalDuration = FormatMinutesToHours(totalMinutes);

        bool isPrerequisiteCompleted = false;

        if (lesson.Prerequisite != null && currentStudentId.HasValue)
        {
            var prereqSpec = new LessonWithDetailsSpecification(lesson.PrerequisiteId.Value);
            var prereqLesson = await lessonrepository.FirstOrDefaultAsync(prereqSpec, cancellationToken);

            if (prereqLesson != null)
            {
                bool isEnrolled = prereqLesson.Enrollments?
                    .Any(e => e.StudentId == currentStudentId.Value && e.Status == EnrollmentStatus.Active) ?? false;

                if (isEnrolled)
                {
                    int totalPrereqQuizzes = prereqLesson.Quizzes?.Count ?? 0;

                    if (totalPrereqQuizzes > 0)
                    {
                        int submittedQuizzesCount = prereqLesson.Quizzes?
                            .Count(q => q.Attempts != null &&
                                        q.Attempts.Any(a => a.StudentId == currentStudentId.Value &&
                                                            a.Status == QuizAttemptStatus.Submitted)) ?? 0;

                        if (submittedQuizzesCount == totalPrereqQuizzes)
                        {
                            isPrerequisiteCompleted = true;
                        }
                    }
                    else
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