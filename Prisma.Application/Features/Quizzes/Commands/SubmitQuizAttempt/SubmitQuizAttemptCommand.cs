using Ardalis.Result;
using Prisma.Application.Abstractions.Authorization;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Commands.SubmitQuizAttempt;

public record SubmitQuizAttemptCommand(int AttemptId, IList<SubmitAnswerDto>? Answers)
    : ICommand<Result<SubmitQuizResultDto>>,
        IAuthorizableRequest
{
    public IReadOnlyList<string> RequiredRoles { get; } = [AppRoles.Student];
    public IReadOnlyList<string> RequiredPermissions { get; }
}
