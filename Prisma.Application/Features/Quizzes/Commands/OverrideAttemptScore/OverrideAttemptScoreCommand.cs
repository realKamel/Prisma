using MediatR;
using Prisma.Application.Common.Responses;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Commands.OverrideAttemptScore;

public record OverrideAttemptScoreCommand(
    int AttemptId,
    decimal PenaltyScore
) : IRequest<Result<OverrideScoreResultDto>>;