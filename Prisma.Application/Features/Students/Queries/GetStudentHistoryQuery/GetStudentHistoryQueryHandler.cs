using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.DTOs;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Enrollments;

namespace Prisma.Application.Features.Students.Queries.GetStudentHistoryQuery;

internal class GetStudentHistoryQueryHandler(
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork
) : IRequestHandler<GetPaginatedStudentHistoryQuery, Result<PaginatedList<HistoryDto>>>
{
    public async Task<Result<PaginatedList<HistoryDto>>> Handle(
        GetPaginatedStudentHistoryQuery request,
        CancellationToken cancellationToken
    )
    {
        var userId = currentUserService.UserId;

        if (userId is null)
        {
            return Result.Unauthorized();
        }

        var repo = unitOfWork.GetOrCreateRepository<Enrollment, int>();

        var totalCount = await repo.CountAsync(
            new EnrollmentByStudentSpecification(userId.Value),
            cancellationToken
        );

        if (totalCount == 0)
        {
            return new PaginatedList<HistoryDto>(
                [],
                0,
                request.PaginationParams.PageNumber,
                request.PaginationParams.PageSize
            );
        }

        var rawData = await repo.ListAsync(
            new PaginatedEnrollmentHistorySpecification<EnrollmentHistoryDto>(
                userId.Value,
                request.PaginationParams.PageNumber,
                request.PaginationParams.PageSize,
                e => new EnrollmentHistoryDto(
                    e.Lesson != null ? e.Lesson.Id : 0,
                    e.Lesson != null ? (e.Lesson.ImageThumbnailUrl ?? string.Empty) : string.Empty,
                    e.Lesson != null ? (e.Lesson.Title ?? string.Empty) : string.Empty,
                    e.Status.ToString(),
                    e.Lesson != null && e.Lesson.Teacher != null
                        ? $"{e.Lesson.Teacher.FirstName} {e.Lesson.Teacher.SecondName}"
                        : string.Empty,
                    e.Lesson != null && e.Lesson.Teacher != null
                        ? (e.Lesson.Teacher.Subject ?? string.Empty)
                        : string.Empty,
                    e.CreatedAt,
                    e.CompletedAt,
                    e.ExpiresAt,
                    e.Lesson != null && e.Lesson.Quiz != null ? e.Lesson.Quiz.TotalDegree : 0,
                    e.IsCompleted,
                    e.Lesson != null ? e.Lesson.Sections.Count : 0,
                    e.Lesson != null
                        ? e.Lesson.Sections.SelectMany(sec => sec.Progresses)
                            .Where(p => p.StudentId == userId.Value)
                            .Sum(p => (double?)p.Percentage) ?? 0
                        : 0
                )
            ),
            cancellationToken: cancellationToken
        );

        var result = rawData
            .Select(raw => new HistoryDto(
                raw.PublicId,
                raw.ImageThumbnailUrl,
                raw.Title,
                raw.Status,
                raw.TeacherName,
                raw.Subject,
                raw.CreatedAt,
                raw.CompletedAt,
                raw.ExpiresAt,
                raw.TotalDegree,
                CalculateProgressPercentage(raw.IsCompleted, raw.SectionsCount, raw.TotalProgress)
            ))
            .ToList();
        return new PaginatedList<HistoryDto>(
            result,
            totalCount,
            request.PaginationParams.PageNumber,
            request.PaginationParams.PageSize
        );
    }

    private static double CalculateProgressPercentage(
        bool isCompleted,
        int sectionsCount,
        double totalProgress
    )
    {
        if (isCompleted)
        {
            return 100;
        }

        if (sectionsCount == 0)
        {
            return 0;
        }

        return totalProgress / sectionsCount;
    }
}
