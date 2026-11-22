using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonMaterial;

public class GetLessonMaterialQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : IQueryHandler<GetLessonMaterialQuery, Result<List<LessonMaterialDto>>>
{
    private static readonly IReadOnlyList<string> MaterialViewerRoles = new[]
    {
        AppRoles.Teacher,
        AppRoles.Assistant,
        AppRoles.Student,
        AppRoles.Admin,
    };

    public async Task<Result<List<LessonMaterialDto>>> Handle(
        GetLessonMaterialQuery request,
        CancellationToken cancellationToken
    )
    {
        if (!currentUserService.IsAuthenticated || currentUserService.UserId is not { } userId)
        {
            return Result.Unauthorized();
        }

        if (!currentUserService.IsInAnyRole(MaterialViewerRoles, cancellationToken))
        {
            return Result.Unauthorized("You are not authorized to view lesson materials.");
        }

        var lessonRepository = unitOfWork.GetOrCreateRepository<Lesson, int>();
        var spec = new LessonMaterialsSpecification(request.LessonId);

        var lessonMaterials = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (lessonMaterials is null)
        {
            return Result.NotFound($"Lesson with id '{request.LessonId}' was not found");
        }

        var materials = lessonMaterials
            .Select(material => new LessonMaterialDto(
                material.Id,
                material.Title,
                material.Size,
                material.Type,
                material.CreatedAt
            ))
            .ToList();

        return Result<List<LessonMaterialDto>>.Success(materials);
    }
}
