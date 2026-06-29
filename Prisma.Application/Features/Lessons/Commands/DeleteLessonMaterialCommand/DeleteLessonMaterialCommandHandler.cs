using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Lessons.Commands.UploadLessonMaterials;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.Teachers;

namespace Prisma.Application.Features.Lessons.Commands.DeleteLessonMaterialCommand;

public class DeleteLessonMaterialCommandHandler
(IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    UserManager<User> _userManager,
    IStorageService storageService
) : IRequestHandler<DeleteLessonMaterialCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteLessonMaterialCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User must be authenticated.");
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new UnauthorizedException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(AppRoles.Teacher) && !roles.Contains(AppRoles.Assistant))
            throw new UnauthorizedException("Only teachers and assistants can delete materials from lessons.");
        
        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        var spec = new LessonMaterialsSpecification(request.LessonId);
        var lesson = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (lesson is null)
            throw new NotFoundException("Lesson material", request.MaterialId);

        LessonMaterial materialToDelete =  lesson.LessonMaterials.FirstOrDefault(m => m.Id == request.MaterialId);
        if (materialToDelete != null)
        {
            
            await storageService.DeleteFileAsync("prisma", materialToDelete.DownloadUrl, cancellationToken);
            
            lesson.LessonMaterials.Remove(materialToDelete);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        return Result<string>.Success("Material deleted successfully.");
    }
}    