using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.TeacherStudents.Dtos;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetTeacherLessonsForGrant;

public class GetTeacherLessonsForGrantQueryHandler(IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IIdentityService identityService) : IRequestHandler<GetTeacherLessonsForGrantQuery, List<LessonForGrantDto>>
{
    public async Task<List<LessonForGrantDto>> Handle(GetTeacherLessonsForGrantQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            return Result<List<LessonForGrantDto>>.Unauthorized("User is not authenticated.");

        var user = await identityService.FindByIdAsync(userId.Value, cancellationToken);
        if (user is null)
            return Result<List<LessonForGrantDto>>.NotFound("User not found.");

        if (user is Assistant assistant)
        {
            if (assistant.TeacherId is null)
                return Result<List<LessonForGrantDto>>.Unauthorized("Assistant is not associated with a teacher.");
            userId = assistant.TeacherId;
        }

        var lessonRepo = unitOfWork.GetOrCreateRepository<Domain.Entities.LessonAggregate.Lesson, int>();
        
        var lessons = await lessonRepo.ListAsync(new LessonWithProjectionSpec<LessonForGrantDto>(Guid.Empty, l => new LessonForGrantDto(
            l.Id,
            l.Title ?? "—",
            l.Sections.Count.ToString()
        )), cancellationToken);

        return lessons.ToList();
    }
}