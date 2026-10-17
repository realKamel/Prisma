using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.DTOs;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonExpired;

internal sealed class GetLessonExpiredQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : IRequestHandler<GetLessonExpiredQuery, Result<LessonExpiredDto>>
{
    public async Task<Result<LessonExpiredDto>> Handle(
        GetLessonExpiredQuery request,
        CancellationToken cancellationToken
    )
    {
        Guid? currentStudentId = currentUserService.UserId;
        if (currentStudentId is null)
            return Result.Unauthorized("User is not authenticated");

        var lessonRepository = unitOfWork.GetOrCreateRepository<Lesson, int>();
        var spec = new LessonExpiredSpecification(request.LessonId, currentStudentId.Value);
        var lesson = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (lesson == null)
        {
            return Result.NotFound($"Lesson with id '{request.LessonId}' was not found");
        }

        var lessonExpiredDto = new LessonExpiredDto
        {
            Id = lesson.Id,
            Url = lesson.ImageThumbnailUrl ?? string.Empty,
            Title = lesson.Title,
            Subject = lesson.TeacherSubject ?? string.Empty,
            Description = lesson.Description ?? string.Empty,
            ChaptersCount = lesson.ChaptersCount,
            Price = lesson.Price,
            MaterialsCount = lesson.MaterialsCount,
            totalprogress = lesson.TotalProgress,
            Degree = lesson.Degree,
            ExpiredDate = lesson.ExpiredDate,
            ValidityDays = 7,
            Chapters = lesson
                .Chapters.Select(s => new ChapterDto(
                    s.Id,
                    s.Title ?? "",
                    s.Duration?.ToDto() ?? new TimeDurationDto(0)
                ))
                .ToList(),
        };

        return Result<LessonExpiredDto>.Success(lessonExpiredDto);
    }
}
