using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.DTOs;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.ValueObjects.ContentDomain;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherLessonsQuery;

internal sealed class GetTeacherLessonsQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IIdentityService identityService,
    IStorageService storageService
) : IRequestHandler<GetTeacherLessonsQuery, Result<TeacherLessonsResponse>>
{
    public async Task<Result<TeacherLessonsResponse>> Handle(
        GetTeacherLessonsQuery request,
        CancellationToken cancellationToken
    )
    {
        var userId = currentUserService.UserId;
        if (userId is null)
        {
            return Result.Unauthorized();
        }

        var user = await identityService.FindByIdAsync(userId.Value, cancellationToken);

        if (user is null)
            return Result.NotFound();

        if (user is Assistant assistant)
        {
            if (assistant.TeacherId is null)
                return Result.Unauthorized("Assistant is not associated with a teacher.");
            userId = assistant.TeacherId;
        }

        var teacherId = userId.Value;
        var lessonRepository = unitOfWork.GetOrCreateRepository<Lesson, int>();

        var spec = new TeacherLessonsWithProjectionSpec<TeacherLessonsInfo>(
            teacherId,
            request.PaginationParams.PageNumber,
            request.PaginationParams.PageSize,
            request.Search,
            request.Status,
            request.Sort,
            e => new TeacherLessonsInfo(
                e.Id,
                e.Title,
                e.Price,
                e.Money,
                e.Enrollments.Count,
                e.Status,
                e.ImageThumbnailUrl,
                e.Sections.Count,
                e.TimeDuration,
                e.Enrollments.Count(x => x.IsCompleted),
                e.Payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Money.Amount),
                e.CreatedAt.Value,
                e.UpdatedAt
            )
        );

        var lessons = await lessonRepository.ListAsync(spec, cancellationToken);
        var count = await lessonRepository.CountAsync(spec, cancellationToken);
        var statuses = await lessonRepository.ListAsync(
            new TeacherLessonStatusesSpec(teacherId),
            cancellationToken
        );

        var items = new List<TeacherLessonDto>(lessons.Count);
        foreach (var l in lessons)
        {
            items.Add(
                new TeacherLessonDto
                {
                    Id = l.Id,
                    Name = l.Name ?? string.Empty,
                    Price = l.Price,
                    Money = l.Money.ToDto(),
                    Students = l.Students,
                    Status = l.Status,
                    ThumbnailUrl = await ResolveThumbnailUrlAsync(
                        l.ThumbnailUrl,
                        cancellationToken
                    ),
                    SectionsCount = l.SectionsCount,
                    DurationSeconds = (int)l.Duration.Value.TotalSeconds,
                    CompletionRate =
                        l.Students == 0
                            ? null
                            : (int)Math.Round(100.0 * l.CompletedCount / l.Students),
                    Revenue = l.Revenue,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt,
                }
            );
        }

        var counts = new TeacherLessonStatusCounts(
            All: statuses.Count,
            Active: statuses.Count(s => s == LessonStatus.Active),
            Hidden: statuses.Count(s => s == LessonStatus.Hidden),
            Drafted: statuses.Count(s => s == LessonStatus.Drafted)
        );

        return new TeacherLessonsResponse(
            new PaginatedList<TeacherLessonDto>(
                items,
                count,
                request.PaginationParams.PageNumber,
                request.PaginationParams.PageSize
            ),
            counts
        );
    }

    private async Task<string?> ResolveThumbnailUrlAsync(string? objectKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
            return null;

        return await storageService.GetDownloadUrlAsync(objectKey);
    }

    public sealed record TeacherLessonsInfo(
        int Id,
        string? Name,
        decimal Price,
        Money Money,
        int Students,
        LessonStatus Status,
        string? ThumbnailUrl,
        int SectionsCount,
        TimeDuration Duration,
        int CompletedCount,
        decimal Revenue,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt
    );
}