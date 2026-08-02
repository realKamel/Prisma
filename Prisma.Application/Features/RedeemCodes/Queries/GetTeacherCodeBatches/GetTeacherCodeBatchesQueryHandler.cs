using MediatR;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Application.Features.RedeemCodes.Dtos;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.RedeemCodes;

namespace Prisma.Application.Features.RedeemCodes.Queries.GetTeacherCodeBatches;

public class GetTeacherCodeBatchesQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser)
    : IRequestHandler<GetTeacherCodeBatchesQuery, Result<List<CodeBatchListItemDto>>>
{
    public async Task<Result<List<CodeBatchListItemDto>>> Handle(
        GetTeacherCodeBatchesQuery request,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            return Result.Unauthorized("User is not authenticated.");

        var repo = unitOfWork.GetOrCreateRepository<RedeemCode, int>();

        var batches = await repo.ListAsync(
            new TeacherCodeBatchesSpecification(request.AcademicYearId, request.LessonId), ct);

        var result = batches.Select(b => new CodeBatchListItemDto
        {
            Id = b.Id,
            AcademicYearId = b.AcademicYearId,
            AcademicYear = b.AcademicYear.Title,
            LessonId = b.LessonId,
            Lesson = b.Lesson.Title ?? string.Empty,
            CreatedAt = b.CreatedAt?.ToString("yyyy/MM/dd") ?? string.Empty,
            TotalCodes = b.TotalCodes,
            UsedCodes = b.GeneratedCodes.Count(c => c.RedeemedByStudentId != null),
        }).ToList();

        return result;
    }
}