using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Commands.OverrideAttemptScore;

public record OverrideAttemptScoreCommand(int AttemptId, decimal PenaltyScore)
    : IRequest<Result<OverrideScoreResultDto>>;
