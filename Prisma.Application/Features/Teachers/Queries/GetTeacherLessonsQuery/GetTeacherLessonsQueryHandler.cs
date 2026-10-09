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
    IIdentityService identityService
) : IRequestHandler<GetTeacherLessonsQuery, Result<PaginatedList<TeacherLessonDto>>>
{
    public async Task<Result<PaginatedList<TeacherLessonDto>>> Handle(
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

        var lessonRepository = unitOfWork.GetOrCreateRepository<Lesson, int>();
        //var spec = new TeacherLessonsSpecification();
        var spec = new TeacherLessonsWithProjectionSpec<TeacherLessonsInfo>(
            userId.Value,
            request.PaginationParams.PageNumber,
            request.PaginationParams.PageSize,
            e => new TeacherLessonsInfo(
                e.Id,
                e.Title,
                e.Price,
                e.Money,
                e.Enrollments.Count,
                e.Status
            )
        );

        var lessons = await lessonRepository.ListAsync(spec, cancellationToken);
        var count = await lessonRepository.CountAsync(spec, cancellationToken);

        var result = lessons
            .Select(lesson =>
            {
                var price = lesson.Money.ToDto();
                return new TeacherLessonDto
                {
                    Id = lesson.Id,
                    Name = lesson.Name ?? string.Empty,
                    Price = lesson.Price,
                    Money = price,
                    Students = lesson.Students,
                    Status = lesson.Status,
                };
            })
            .ToList();

        var paginatedList = new PaginatedList<TeacherLessonDto>(
            result,
            count,
            request.PaginationParams
        );

        return paginatedList;
    }

    public sealed record TeacherLessonsInfo(
        int Id,
        string? Name,
        decimal Price,
        Money Money,
        int Students,
        LessonStatus Status
    );
}
