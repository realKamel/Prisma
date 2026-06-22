using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherLessons;

public class GetTeacherLessonsQueryHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService)
    : IRequestHandler<GetTeacherLessonsQuery, Result<List<TeacherLessonDto>>>
{
    public async Task<Result<List<TeacherLessonDto>>> Handle(
        GetTeacherLessonsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            if (userId is null)
                return Result<List<TeacherLessonDto>>.Failure("User is not authenticated");
        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();

        var spec = new TeacherLessonsSpecification();
        var lessons = await lessonRepository.ListAsync(spec, cancellationToken);

        var result = lessons.Select(lesson =>
        {
            

            return new TeacherLessonDto
            {
                Id = lesson.Id,
                Name = lesson.Title ?? string.Empty,
                Price = lesson.Price,

                Students = lesson.Enrollments?.Count ?? 0,

                Status = lesson.Status.ToString().ToLowerInvariant()
            };
        }).ToList();

        return Result<List<TeacherLessonDto>>.Success(result);
    }
}