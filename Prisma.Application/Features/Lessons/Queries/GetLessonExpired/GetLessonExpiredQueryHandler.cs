using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Application.Features.Lessons.Queries.GetLessonDetails;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonExpired;

public class GetLessonExpiredQueryHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService)
    : IRequestHandler<GetLessonExpiredQuery, Result<LessonExpiredDto>>
{
    public async Task<Result<LessonExpiredDto>> Handle(GetLessonExpiredQuery request, CancellationToken cancellationToken)
    {
        Guid? currentStudentId = _currentUserService.UserId;
        if (currentStudentId is null)
            return Result.Unauthorized("User is not authenticated");

        var lessonrepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        var spec = new LessonExpiredSpecification(request.LessonId);
        var lesson = await lessonrepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (lesson == null)
        {
            return Result.NotFound($"Lesson with id '{request.LessonId.ToString()}' was not found");
        }

        var lessonExpiredDto = new LessonExpiredDto
        {
            Id = lesson.Id,
            Url = lesson.ImageThumbnailUrl ?? string.Empty,
            Title = lesson.Title,
            Subject = "لغه انجليزيه",
            Description = lesson.Description ?? string.Empty,
            ChaptersCount = lesson.Sections.Count,
            Price = lesson.Price,
            MaterialsCount = lesson.LessonMaterials.Count,
            totalprogress = CalculateTotalProgress(lesson, currentStudentId.Value),
            Degree = CalculateDegree(lesson, currentStudentId.Value),
            ExpiredDate = lesson.Enrollments.FirstOrDefault(e => e.StudentId == currentStudentId)?.ExpiresAt,
            ValidityDays = 7,
            Chapters = lesson.Sections?.Select(s => new ChapterDto(
                  s.Id,
                  s.Title ?? "",
                 s.Duration.ToString(@"hh\:mm\:ss")
              )).ToList() ?? [],


        };
        return Result<LessonExpiredDto>.Success(lessonExpiredDto);


    }
    public decimal CalculateDegree(Lesson lesson, Guid studentId)
    {
        if (lesson.Quiz == null) return 0;

        var attempt = lesson.Quiz.Attempts
            .FirstOrDefault(qa => qa.StudentId == studentId && qa.Status == QuizAttemptStatus.Graded && qa.QuizId == lesson.Quiz.Id);

        if (attempt == null) return 0;

        return attempt.Degree;
    }

    public double CalculateTotalProgress(Lesson lesson, Guid studentId)
    {
        var studentProgress = lesson.Sections
            .SelectMany(s => s.Progresses)
            .Where(sp => sp.StudentId == studentId)
            .ToList();

        if (!studentProgress.Any()) return 0;

        return studentProgress.Average(sp => sp.Percentage);
    }
}
