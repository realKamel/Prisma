using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Queries.GetExtractionStatus;

public record GetExtractionStatusQuery(int JobId) 
    : IRequest<Result<ExtractionProgressDto>>;
