using MediatR;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonDetails;

public class GetLessonDetailsQueryHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    IStorageService storageService)
    : IRequestHandler<GetLessonDetailsQuery, Result<LessonDetailsDto>>
{
    public async Task<Result<LessonDetailsDto>> Handle(GetLessonDetailsQuery request,
        CancellationToken cancellationToken)
    {
        Guid? currentStudentId = _currentUserService.UserId;
        if (currentStudentId is null)
            return Result.Unauthorized("User is not authenticated");

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();

        var spec = new LessonWithDetailsSpecification(request.LessonId);
        var lesson = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (lesson == null)
        {
            return Result.NotFound($"Lesson with id '{request.LessonId}' was not found");
        }

        int totalMinutes = (int)lesson.Sections.Sum(s => s.Duration.TotalMinutes);
        string formattedTotalDuration = FormatMinutesToHours(totalMinutes);

        bool isPrerequisiteCompleted = true;

        if (lesson.PrerequisiteId is not null)
        {
            var prereqSpec = new LessonPrerequisiteCompletionSpecification(
                lesson.PrerequisiteId.Value, currentStudentId.Value);

            isPrerequisiteCompleted = await lessonRepository.FirstOrDefaultAsync(prereqSpec, cancellationToken);
        }

        var lessonDto = new LessonDetailsDto
        {
            Id = lesson.Id,
            Url = lesson.ImageThumbnailUrl != null
                ? await storageService.GetDownloadUrlAsync(storageService.DefaultBucketName, lesson.ImageThumbnailUrl)
                : string.Empty,
            Title = lesson.Title ?? "",
            Price = lesson.Price,
            AboutText = lesson.Description ?? "",
            StudentsCount = lesson.EnrollmentsCount,
            ChaptersCount = lesson.Sections.Count,
            Subject = lesson.TeacherSubject,
            Teacher = lesson.TeacherName,
            Duration = formattedTotalDuration,
            ValidityDays = 7,
            Chapters = lesson.Sections.Select(s => new ChapterDto(
                s.Id,
                s.Title ?? "",
                $"{(int)s.Duration.TotalMinutes} د",
                s.IsPreview
            )).ToList(),
            Outcomes = lesson.Outcomes,
            Prerequisites = lesson.PrerequisiteId is not null
                ? [new PrerequisiteDto(lesson.PrerequisiteTitle ?? "", isPrerequisiteCompleted)]
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