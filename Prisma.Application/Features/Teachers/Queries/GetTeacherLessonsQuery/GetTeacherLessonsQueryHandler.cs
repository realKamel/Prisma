using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Teachers.Queries.GetTeacherLessons;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.Teacher;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherLessonsQuery;

public class GetTeacherLessonsQueryHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    IIdentityService identityService)
    : IRequestHandler<GetTeacherLessons.GetTeacherLessonsQuery, Result<List<TeacherLessonDto>>>
{
    public async Task<Result<List<TeacherLessonDto>>> Handle(
        GetTeacherLessons.GetTeacherLessonsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            return Result.Unauthorized("User is not authenticated.");
        
        var user = await identityService.FindByIdAsync(userId.Value, cancellationToken);
        if (user is null)
            return Result.NotFound("User not found.");

        if (user is Assistant assistant)
        {
            if (assistant.TeacherId is null)
                return Result.Unauthorized("Assistant is not associated with a teacher.");
            userId = assistant.TeacherId;
        }

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        //var spec = new TeacherLessonsSpecification();
        var spec = new TeacherLessonsWithProjectionSpec<TeacherLessonsInfo>(userId.Value, e => new TeacherLessonsInfo
        (
            e.Id,
            e.Title,
            e.Price,
            e.Enrollments!.Count,
            e.Status
        ));

        var lessons = await lessonRepository.ListAsync(spec, cancellationToken);

        var result = lessons.Select(lesson =>
        {
            return new TeacherLessonDto
            {
                Id = lesson.Id,
                Name = lesson.Name ?? string.Empty,
                Price = lesson.Price,
                Students = lesson.Students,
                Status = lesson.Status.ToString().ToLowerInvariant()
            };
        }).ToList();

        return Result<List<TeacherLessonDto>>.Success(result);
    }
    public sealed record TeacherLessonsInfo(
        int Id,
        string? Name,
        decimal Price,
        int Students,
        LessonStatus Status
    );
}