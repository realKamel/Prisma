using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assistants;
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
            return Result.Unauthorized();

        var userId = currentUserService.UserId.Value;

        Guid teacherId = Guid.Empty;

        // 1. Resolve Teacher ID based on Role
        if (currentUserService.Role == AppRoles.Teacher)
        {
            teacherId = userId;
        }
        else if (currentUserService.Role == AppRoles.Assistant)
        {
            var assistantRepo = unitOfWork.GetOrCreateRepository<Assistant, Guid>();
            var teacherAssignmentId = await assistantRepo.FirstOrDefaultAsync(
                new AssistantByIdWithProjectionSpec<Guid?>(userId, x => x.TeacherId),
                cancellationToken
            );

            if (teacherAssignmentId is null || teacherAssignmentId == Guid.Empty)
            {
                return Result.Error("This assistant is not assigned to a teacher.");
            }

            teacherId = teacherAssignmentId.Value;
        }
        else if (currentUserService.Role == AppRoles.Admin)
        {
            // If Admin creates it, ensure your command supports passing a TeacherId,
            // or handle it according to your admin business logic.
            return Result.Error("Admin lesson creation flow needs explicit teacher assignment.");
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
