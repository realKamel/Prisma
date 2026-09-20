using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assistants;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Assistants.Queries.GetAssistantLessons;

internal sealed class GetAssistantLessonsQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IIdentityService identityService)
    : IRequestHandler<GetAssistantLessonsQuery, Result<List<AssistantLessonDto>>>
{
    public async Task<Result<List<AssistantLessonDto>>> Handle(
        GetAssistantLessonsQuery request,
        CancellationToken cancellationToken)
    {
        var isAuthenticated = currentUserService.IsAuthenticated;

        if (!isAuthenticated || currentUserService.UserId is null)
        {
            return Result.Unauthorized();
        }

        var lessonRepository = unitOfWork.GetOrCreateRepository<Lesson, int>();
        // var assistant = await identityService.FindByIdAsync(currentUserService.UserId!.Value, cancellationToken);
        var assistantRepo = unitOfWork.GetOrCreateRepository<Assistant, Guid>();
        var assistant = await assistantRepo.GetByIdAsync(currentUserService.UserId!.Value, cancellationToken);

        if (assistant?.TeacherId is null)
        {
            return Result.Error("Assistant not found");
        }

        var spec = new LessonWithProjectionSpec<AssistantLessonDto>( assistant.TeacherId.Value,
            lesson =>  
             new AssistantLessonDto
            {
                Id = lesson.Id,
                Title = lesson.Title ?? string.Empty,
                Price = lesson.Price,
                StudentsCount = lesson.Enrollments.Count,
                ChaptersCount = lesson.Sections.Count,
                LastUpdatedAt = lesson.UpdatedAt ?? lesson.CreatedAt,
                Status = lesson.Status.ToString().ToLowerInvariant()
            });


        var lessons = await lessonRepository.ListAsync(spec, cancellationToken);


        return lessons;
    }
}