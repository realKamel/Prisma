using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Commands.DeattachLessonAssignment;

internal sealed class DeattachLessonAssignmentCommandHandler(
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeattachLessonAssignmentCommand, Result>
{
    public async Task<Result> Handle(
        DeattachLessonAssignmentCommand request,
        CancellationToken cancellationToken
    )
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            return Result.Unauthorized();
        }

        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();

        var lesson = await lessonRepo.FirstOrDefaultAsync(
            new LessonByPublicIdWithAssignmentSpec(request.LessonId),
            cancellationToken
        );

        if (lesson is null)
        {
            return Result.NotFound("The Requested lesson doesn't exit");
        }

        if (lesson.AssignmentId is null || lesson.Assignment is null)
        {
            return Result.NoContent();
        }

        lesson.RemoveAssignment();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
