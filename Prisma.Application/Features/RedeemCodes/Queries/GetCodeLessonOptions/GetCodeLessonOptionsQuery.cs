using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.RedeemCodes.Dtos;

namespace Prisma.Application.Features.RedeemCodes.Queries.GetCodeLessonOptions;

public record GetCodeLessonOptionsQuery : IRequest<Result<List<CodeLessonOptionDto>>>;