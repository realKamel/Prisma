using MediatR;
using Ardalis.Result;
using Ardalis.Result;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Commands.OverrideAttemptScore;

public record OverrideAttemptScoreCommand(
    int AttemptId,
    decimal PenaltyScore
) : IRequest<Result<OverrideScoreResultDto>>;