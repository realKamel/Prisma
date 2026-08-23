using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.RedeemCodes.Dtos;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.RedeemCodes;

namespace Prisma.Application.Features.RedeemCodes.Queries.GetTeacherCodeBatches;

public class GetTeacherCodeBatchesQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser ,
    IIdentityService identityService)
    : IRequestHandler<GetTeacherCodeBatchesQuery, Result<List<CodeBatchListItemDto>>>
{
    public async Task<Result<List<CodeBatchListItemDto>>> Handle(
        GetTeacherCodeBatchesQuery request,
        CancellationToken ct)
    {
        var userId = currentUser.UserId;
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


        var repo = unitOfWork.GetOrCreateRepository<RedeemCode, int>();

        var batches = await repo.ListAsync(
            new TeacherCodeBatchesSpecification<CodeBatchinfo>(userId.Value,
            b => new CodeBatchinfo
            (
                b.Id,
                b.AcademicYearId,
                b.AcademicYear.Title,
                b.LessonId,
                b.Lesson.Title ?? string.Empty,
                b.CreatedAt ,
                b.TotalCodes,
                b.GeneratedCodes.Count(c => c.RedeemedByStudentId != null)
            ), request.AcademicYearId, request.LessonId), ct);

        var result = batches.Select(b => new CodeBatchListItemDto
        {
            Id = b.Id,
            AcademicYearId = b.AcademicYearId,
            AcademicYear = b.AcademicYear,
            LessonId = b.LessonId,
            Lesson = b.Lesson,
            CreatedAt = b.CreatedAt?.ToString("yyyy/MM/dd") ?? string.Empty,
            TotalCodes = b.TotalCodes,
            UsedCodes = b.UsedCodes,
        }).ToList();

        return result;
    }
}
public record CodeBatchinfo(
    int Id,
    int AcademicYearId,
    string AcademicYear,
    int LessonId,
    string Lesson,
 DateTimeOffset? CreatedAt,
    int TotalCodes,
    int UsedCodes
);
