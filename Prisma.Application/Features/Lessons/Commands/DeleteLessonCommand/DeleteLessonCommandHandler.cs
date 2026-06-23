using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using static Prisma.Application.Common.Constants.AppClaims;

namespace Prisma.Application.Features.Lessons.Commands.DeleteLessonCommand;

public class DeleteLessonCommandHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    UserManager<User> _userManager)
    : IRequestHandler<DeleteLessonCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User must be authenticated to delete a lesson.");

        var user = await _userManager.FindByIdAsync(userId.ToString());


        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(AppRoles.Teacher))
            throw new UnauthorizedException("Only teachers can delete lessons");


        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();

        var lesson = await lessonRepository.GetByIdAsync(request.LessonId, cancellationToken);
        if (lesson is null || lesson.IsDeleted)
            throw new NotFoundException("Lesson", request.LessonId);

        lessonRepository.Delete(lesson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Lesson deleted successfully (Soft Delete)");
    }
}