using MediatR;
using Prisma.Application.Common.Responses;

namespace Prisma.Application.Features.Quizzes.Commands.DeleteQuiz;

public sealed record DeleteQuizCommand(int QuizId): IRequest<Result>;
