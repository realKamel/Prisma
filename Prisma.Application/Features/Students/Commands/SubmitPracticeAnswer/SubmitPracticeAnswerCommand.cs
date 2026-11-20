using Ardalis.Result;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Features.Students.Common;

namespace Prisma.Application.Features.Students.Commands.SubmitPracticeAnswer;

public sealed record SubmitPracticeAnswerCommand(int QuestionId, int ChoiceId)
    : ICommand<Result<PracticeAnswerResultDto>>;