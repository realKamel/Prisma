// التأكد من وجود الـ Enum هنا
using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Lessons.Commands.ToggleLessonStatusCommand;

public class ToggleLessonStatusCommandHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    IIdentityService identityService
) : IRequestHandler<ToggleLessonStatusCommand, Result>
{
    public async Task<Result> Handle(
        ToggleLessonStatusCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            return Result.Unauthorized("User must be authenticated.");

        var user = await identityService.FindByIdAsync(userId.Value, cancellationToken);

        if (user is null)
            return Result.Unauthorized("User not found.");

        var roles = await identityService.GetRolesAsync(user);
        if (!roles.Contains(AppRoles.Teacher) && !roles.Contains(AppRoles.Admin))
            return Result.Unauthorized("Only teachers can toggle lesson status.");

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        var lesson = await lessonRepository.GetByIdAsync(request.Id, cancellationToken);

        if (lesson is null || lesson.IsDeleted)
            return Result.NotFound($"Lesson with id '{request.Id}' was not found");

        if (lesson.Status == LessonStatus.Drafted)
        {
            return Result.Error("Cannot toggle status for a drafted lesson.");
        }

        lesson.Status =
            lesson.Status == LessonStatus.Hidden ? LessonStatus.Active : LessonStatus.Hidden;

        lessonRepository.Update(lesson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessWithMessage($"Lesson status toggled successfully to {lesson.Status}");
    }
}
