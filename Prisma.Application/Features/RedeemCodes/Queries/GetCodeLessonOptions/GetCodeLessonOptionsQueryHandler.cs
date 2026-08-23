using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.RedeemCodes.Dtos;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.RedeemCodes;

namespace Prisma.Application.Features.RedeemCodes.Queries.GetCodeLessonOptions;

public class GetCodeLessonOptionsQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IIdentityService identityService)
    : IRequestHandler<GetCodeLessonOptionsQuery, Result<List<CodeLessonOptionDto>>>
{
    public async Task<Result<List<CodeLessonOptionDto>>> Handle(
        GetCodeLessonOptionsQuery request,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            return Result.Unauthorized("User is not authenticated.");

        var user = await identityService.FindByIdAsync(userId.Value, ct);
        if (user is null)
            return Result.NotFound("User not found.");

        if (user is Assistant assistant)
        {
            if (assistant.TeacherId is null)
                return Result.Unauthorized("Assistant is not associated with a teacher.");
            userId = assistant.TeacherId;
        }

        var repo = unitOfWork.GetOrCreateRepository<AcademicYearLesson, int>();

        var links = await repo.ListAsync(
            new TeacherAcademicYearLessonsSpecification(userId.Value), ct);

        // Deduplicate by (LessonId, AcademicYearId) in case of duplicate join rows,
        // then project — one entry per lesson per academic year.
        var result = links
            .GroupBy(x => new { x.LessonId, x.AcademicYearId })
            .Select(g => g.First())
            .Select(x => new CodeLessonOptionDto
            {
                Id = x.LessonId, Name = x.Lesson.Title ?? string.Empty, AcademicYearId = x.AcademicYearId,
            })
            .ToList();

        return result;
    }
}