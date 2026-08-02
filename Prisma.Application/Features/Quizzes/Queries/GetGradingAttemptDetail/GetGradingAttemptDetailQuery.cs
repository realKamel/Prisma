using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Queries.GetGradingAttemptDetail;

public record GetGradingAttemptDetailQuery(int AttemptId)
    : IRequest<Result<GradingAttemptDetailDto>>;
