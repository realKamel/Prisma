using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assistants;

namespace Prisma.Application.Features.Assistants.Queries.GetAssistantLessons;

public class GetAssistantLessonsQueryHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService)
    : IRequestHandler<GetAssistantLessonsQuery, Result<List<AssistantLessonDto>>>
{
    public async Task<Result<List<AssistantLessonDto>>> Handle(
        GetAssistantLessonsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User is not authenticated.");
        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();

        var spec = new AssistantLessonsSpec();
        var lessons = await lessonRepository.ListAsync(spec, cancellationToken);

        var result = lessons.Select(lesson =>
        {
            return new AssistantLessonDto
            {
                Id = lesson.Id,
                Title = lesson.Title ?? string.Empty,
                Price = lesson.Price,
                StudentsCount = lesson.Enrollments?.Count ?? 0,
                ChaptersCount = lesson.Sections?.Count ?? 0,
                LastUpdatedAt = lesson.UpdatedAt ?? lesson.CreatedAt,
                Status = lesson.Status.ToString().ToLowerInvariant()
            };
        }).ToList();

        return Result<List<AssistantLessonDto>>.Success(result);
    }
}