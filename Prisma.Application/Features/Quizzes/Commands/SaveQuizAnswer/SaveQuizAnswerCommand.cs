using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Quizzes.Commands.SaveQuizAnswer;

public record SaveQuizAnswerCommand(int AttemptId, int QuestionId, int? ChoiceId, string? TextAnswer)
    : IRequest<Result>;