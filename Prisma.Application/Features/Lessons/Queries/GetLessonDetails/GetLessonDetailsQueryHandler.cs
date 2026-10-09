using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.DTOs;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonDetails;

internal sealed class GetLessonDetailsQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IStorageService storageService
) : IRequestHandler<GetLessonDetailsQuery, Result<LessonDetailsDto>>
{
    public async Task<Result<LessonDetailsDto>> Handle(
        GetLessonDetailsQuery request,
        CancellationToken cancellationToken
    )
    {
        Guid? currentStudentId = currentUserService.UserId;

        if (currentStudentId is null)
        {
            return Result.Unauthorized();
        }

        var lessonRepository = unitOfWork.GetOrCreateRepository<Lesson, int>();

        var spec = new LessonWithDetailsSpecification(request.LessonId);

        var lesson = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (lesson == null)
        {
            return Result.NotFound($"Lesson with id '{request.LessonId}' was not found");
        }

        int totalSeconds = (int)lesson.Sections.Sum(s => s.Duration.TotalSeconds);

        bool isPrerequisiteCompleted = true;

        if (lesson.PrerequisiteId is not null)
        {
            var prereqSpec = new LessonPrerequisiteCompletionSpecification(
                lesson.PrerequisiteId.Value,
                currentStudentId.Value
            );

            isPrerequisiteCompleted = await lessonRepository.FirstOrDefaultAsync(
                prereqSpec,
                cancellationToken
            );
        }

        var money = lesson.Money.ToDto();

        var lessonDto = new LessonDetailsDto
        {
            Id = lesson.Id,
            Url =
                lesson.ImageThumbnailUrl != null
                    ? await storageService.GetDownloadUrlAsync(
                        storageService.DefaultBucketName,
                        lesson.ImageThumbnailUrl
                    )
                    : string.Empty,
            Title = lesson.Title ?? "",
            Money = money,
            AboutText = lesson.Description ?? "",
            StudentsCount = lesson.EnrollmentsCount,
            ChaptersCount = lesson.Sections.Count,
            Subject = lesson.TeacherSubject,
            Teacher = lesson.TeacherName,
            Duration = new TimeDurationDto(totalSeconds),
            ValidityDays = 7,
            ValidityDateRange = lesson.ValidityRange?.ToDto(),
            Chapters =
            [
                .. lesson.Sections.Select(s => new ChapterDto(
                    s.Id,
                    s.Title ?? "",
                    new TimeDurationDto((int)s.Duration.TotalSeconds),
                    s.IsPreview
                )),
            ],
            Outcomes = lesson.Outcomes,
            Prerequisites = lesson.PrerequisiteId is not null
                ? [new PrerequisiteDto(lesson.PrerequisiteTitle ?? "", isPrerequisiteCompleted)]
                : [],
        };

        return Result<LessonDetailsDto>.Success(lessonDto);
    }
}
