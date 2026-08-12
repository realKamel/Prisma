using MediatR;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Application.Features.RedeemCodes.Dtos;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.RedeemCodes;

namespace Prisma.Application.Features.RedeemCodes.Queries.GetCodeLessonOptions;

public class GetCodeLessonOptionsQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser)
    : IRequestHandler<GetCodeLessonOptionsQuery, Result<List<CodeLessonOptionDto>>>
{
    public async Task<Result<List<CodeLessonOptionDto>>> Handle(
        GetCodeLessonOptionsQuery request,
        CancellationToken ct)
    {
        if (!currentUser.UserId.HasValue)
        {
            return Result.Unauthorized();
        }

        var teacherId = currentUser.UserId.Value;

        var repo = unitOfWork.GetOrCreateRepository<AcademicYearLesson, int>();

        var links = await repo.ListAsync(
            new TeacherAcademicYearLessonsSpecification(teacherId), ct);

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