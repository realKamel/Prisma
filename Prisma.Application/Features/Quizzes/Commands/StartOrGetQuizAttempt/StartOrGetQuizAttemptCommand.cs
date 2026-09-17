using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Commands.StartOrGetQuizAttempt;

public record StartOrGetQuizAttemptCommand(int QuizId) : IRequest<Result<AttemptStateDto>>;
