using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Quizzes.Commands.DeleteQuiz;

public sealed record DeleteQuizCommand(int QuizId): IRequest<Result>;
