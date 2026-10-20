
using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Commands.SubmitQuizAttempt;

public record SubmitQuizAttemptCommand(int AttemptId, List<SubmitAnswerDto>? Answers) : IRequest<Result<SubmitQuizResultDto>>;

