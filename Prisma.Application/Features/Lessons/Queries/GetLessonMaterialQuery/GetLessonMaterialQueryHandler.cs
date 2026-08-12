using System.Globalization;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonMaterialQuery;

public class GetLessonMaterialQueryHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    UserManager<User> _userManager
) : IRequestHandler<GetLessonMaterialQuery, Result<List<LessonMaterialDto>>>
{
    public async Task<Result<List<LessonMaterialDto>>> Handle(GetLessonMaterialQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            return Result.Unauthorized("User must be authenticated.");

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null)
            return Result.Unauthorized("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(AppRoles.Teacher) && !roles.Contains(AppRoles.Assistant) && !roles.Contains(AppRoles.Student) && !roles.Contains(AppRoles.Admin))
            return Result.Unauthorized("You are not authorized to view lesson materials.");

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        var spec = new LessonMaterialsSpecification(request.LessonId);

        var lessonMaterials = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (lessonMaterials is null)
            return Result.NotFound($"Lesson with id '{request.LessonId}' was not found");

        var materials = lessonMaterials.Select(material => new LessonMaterialDto(
            material.Id,
            material.Title,
            material.Size,
            material.Type,
            material.CreatedAt?.AddHours(3).ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("ar-EG")) ?? string.Empty
        )).ToList();

        return Result<List<LessonMaterialDto>>.Success(materials);
    }
}