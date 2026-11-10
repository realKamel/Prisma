using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Application.Features.Quizzes.Specifications;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Quizzes.Queries.GetLessonsAvailableForQuiz;

public class GetLessonsAvailableForQuizQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IIdentityService identityService
) : IRequestHandler<GetLessonsAvailableForQuizQuery, Result<List<LessonOptionDto>>>
{
    public async Task<Result<List<LessonOptionDto>>> Handle(
        GetLessonsAvailableForQuizQuery request,
        CancellationToken ct
    )
    {
        var userId = currentUserService.UserId;
        if (userId is null)
        {
            return Result.Unauthorized();
        }

        var user = await identityService.FindByIdAsync(userId.Value, ct);

        if (user is null)
            return Result.NotFound();

        if (user is Assistant assistant)
        {
            if (assistant.TeacherId is null)
                return Result.Unauthorized("Assistant is not associated with a teacher.");
            userId = assistant.TeacherId;
        }

        var teacherId = userId.Value;

        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();

        var lessons = await lessonRepo.ListAsync(
            new LessonsAvailableForQuizSpecification(teacherId),
            ct
        );

        return Result<List<LessonOptionDto>>.Success(lessons);
    }
}
