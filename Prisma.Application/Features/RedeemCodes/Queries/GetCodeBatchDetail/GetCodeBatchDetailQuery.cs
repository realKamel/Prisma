using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.RedeemCodes.Dtos;

namespace Prisma.Application.Features.RedeemCodes.Queries.GetCodeBatchDetail;

public record GetCodeBatchDetailQuery(int BatchId) : IRequest<Result<CodeBatchDetailDto>>;