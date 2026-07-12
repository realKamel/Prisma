using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.RedeemCodes.Dtos;

namespace Prisma.Application.Features.RedeemCodes.Queries.GetTeacherCodeBatches;

public record GetTeacherCodeBatchesQuery(
    int? AcademicYearId,
    int? LessonId
) : IRequest<Result<List<CodeBatchListItemDto>>>;