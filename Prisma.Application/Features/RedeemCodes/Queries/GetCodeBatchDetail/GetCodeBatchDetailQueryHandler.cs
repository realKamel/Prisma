using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.RedeemCodes.Dtos;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.RedeemCodes;

namespace Prisma.Application.Features.RedeemCodes.Queries.GetCodeBatchDetail;

internal class GetCodeBatchDetailQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser)
    : IRequestHandler<GetCodeBatchDetailQuery, Result<CodeBatchDetailDto>>
{
    public async Task<Result<CodeBatchDetailDto>> Handle(
        GetCodeBatchDetailQuery request,
        CancellationToken ct)
    {
        if (currentUser.UserId is not { } teacherId)
            throw new UnauthorizedException("User is not authenticated.");

        var repo = unitOfWork.GetOrCreateRepository<RedeemCode, int>();

        var batch = await repo.GetByIdAsync(request.BatchId, ct);

        // GetByIdAsync won't apply our spec includes, so use FirstOrDefaultAsync via spec
        var spec = new CodeBatchByIdSpecification(request.BatchId, teacherId);
        var batchWithDetails = await repo.FirstOrDefaultAsync(spec, ct);

        if (batchWithDetails is null)
            throw new NotFoundException("Code batch not found.", request.BatchId);

        var dto = new CodeBatchDetailDto
        {
            Id = batchWithDetails.Id,
            AcademicYearId = batchWithDetails.AcademicYearId,
            AcademicYear = batchWithDetails.AcademicYear.Title,
            LessonId = batchWithDetails.LessonId,
            Lesson = batchWithDetails.Lesson.Title ?? string.Empty,
            CreatedAt = batchWithDetails.CreatedAt?.ToString("yyyy/MM/dd") ?? string.Empty,
            TotalCodes = batchWithDetails.TotalCodes,
            UsedCodes = batchWithDetails.GeneratedCodes.Count(c => c.RedeemedByStudentId != null),
            Codes = batchWithDetails.GeneratedCodes
                .OrderBy(c => c.Id)
                .Select(c => new GeneratedCodeDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Status = c.RedeemedByStudentId != null ? "used" : "available",
                    UsedBy = c.RedeemedByStudent != null
                        ? BuildFullName(
                            c.RedeemedByStudent.FirstName,
                            c.RedeemedByStudent.SecondName,
                            c.RedeemedByStudent.ThirdName,
                            c.RedeemedByStudent.LastName)
                        : string.Empty,
                    UsedAt = c.RedeemedAt?.ToString("yyyy/MM/dd") ?? string.Empty,
                })
                .ToList(),
        };

        return dto;
    }

    private static string BuildFullName(string? first, string? second, string? third, string? last)
    {
        return string.Join(" ", new[] { first, second, third, last }
            .Where(p => !string.IsNullOrWhiteSpace(p)));
    }
}