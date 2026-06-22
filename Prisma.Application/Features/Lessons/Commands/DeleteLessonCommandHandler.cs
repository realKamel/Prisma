using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Lessons.Commands.DeleteLesson;

public class DeleteLessonCommandHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService)
    : IRequestHandler<DeleteLessonCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            return Result<string>.Failure("User is not authenticated");

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();

        var lesson = await lessonRepository.GetByIdAsync(request.LessonId, cancellationToken);
        if (lesson is null || lesson.IsDeleted)
            return Result<string>.Failure("Lesson not found or already deleted");

        lesson.IsDeleted = true;
        lesson.DeletedAt = DateTimeOffset.UtcNow;
        lesson.DeletedBy = userId;

        lessonRepository.Update(lesson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Lesson deleted successfully (Soft Delete)");
    }
}