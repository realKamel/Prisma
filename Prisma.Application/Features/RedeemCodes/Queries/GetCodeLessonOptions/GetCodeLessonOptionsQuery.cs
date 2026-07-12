using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.RedeemCodes.Dtos;

namespace Prisma.Application.Features.RedeemCodes.Queries.GetCodeLessonOptions;

public record GetCodeLessonOptionsQuery : IRequest<Result<List<CodeLessonOptionDto>>>;