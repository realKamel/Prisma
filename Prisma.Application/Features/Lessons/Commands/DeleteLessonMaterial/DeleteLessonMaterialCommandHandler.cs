using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Commands.DeleteLessonMaterial;

public sealed class DeleteLessonMaterialCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : ICommandHandler<DeleteLessonMaterialCommand, Result>
{
    public async Task<Result> Handle(
        DeleteLessonMaterialCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = currentUserService.UserId;

        if (userId is null)
        {
            return Result.Unauthorized();
        }

        var lessonRepository = unitOfWork.GetOrCreateRepository<Lesson, int>();

        var spec = new LessonWithMaterialsForUpdateSpecification(request.PublicLessonId);

        var lesson = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (lesson is null)
        {
            return Result.NotFound($"Lesson material with id '{request.MaterialId}' was not found");
        }

        lesson.RemoveMaterial(request.MaterialId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
