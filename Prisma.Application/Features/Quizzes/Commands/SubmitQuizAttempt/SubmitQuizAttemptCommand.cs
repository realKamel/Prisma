
using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Commands.SubmitQuizAttempt;

public record SubmitQuizAttemptCommand(int AttemptId) : IRequest<Result<SubmitQuizResultDto>>;

