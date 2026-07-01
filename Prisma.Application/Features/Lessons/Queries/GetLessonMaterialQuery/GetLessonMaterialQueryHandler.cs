using System.Globalization;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonMaterialQuery;

public class GetLessonMaterialQueryHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    UserManager<User> _userManager,
    IStorageService _storageService
) : IRequestHandler<GetLessonMaterialQuery, Result<List<LessonMaterialDto>>>
{
    public async Task<Result<List<LessonMaterialDto>>> Handle(GetLessonMaterialQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User must be authenticated.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new UnauthorizedException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(AppRoles.Teacher) && !roles.Contains(AppRoles.Assistant) && !roles.Contains(AppRoles.Student) && !roles.Contains(AppRoles.Admin))
            throw new UnauthorizedException("You are not authorized to view lesson materials.");

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        var spec = new LessonMaterialsSpecification(request.LessonId);

        var lesson = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (lesson is null)
            throw new NotFoundException("Lesson", request.LessonId);

        var materials = new List<LessonMaterialDto>();

        foreach (var material in lesson.LessonMaterials)
        {
            var downloadUrl = await _storageService.GetDownloadUrlAsync("prisma", material.DownloadUrl);

            materials.Add(new LessonMaterialDto(
                material.Id,
                material.Title,
                material.Size,
                material.Type.ToString(),
                material.CreatedAt?.AddHours(3).ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("ar-EG")) 
            ));
        }

        return Result<List<LessonMaterialDto>>.Success(materials);
    }
}