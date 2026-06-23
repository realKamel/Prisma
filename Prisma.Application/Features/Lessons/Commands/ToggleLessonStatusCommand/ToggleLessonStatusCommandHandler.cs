// التأكد من وجود الـ Enum هنا
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Lessons.Commands.ToggleLessonStatusCommand;

public class ToggleLessonStatusCommandHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    UserManager<User> _userManager)
    : IRequestHandler<ToggleLessonStatus.ToggleLessonStatusCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ToggleLessonStatus.ToggleLessonStatusCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User must be authenticated.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new UnauthorizedException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(AppRoles.Teacher))
            throw new UnauthorizedException("Only teachers can toggle lesson status.");

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        var lesson = await lessonRepository.GetByIdAsync(request.Id, cancellationToken);

        if (lesson is null || lesson.IsDeleted)
            throw new NotFoundException("Lesson", request.Id);

        if (lesson.Status == LessonStatus.Drafted)
        {
            throw new BadRequestException("Cannot toggle status for a drafted lesson.");
        }

        lesson.Status = lesson.Status == LessonStatus.Hidden
            ? LessonStatus.Active
            : LessonStatus.Hidden;

        lessonRepository.Update(lesson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Success($"Lesson status toggled successfully to {lesson.Status}");
    }
}