
using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Features.Students.Common;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Students.Commands.SubmitPracticeAnswer;

public sealed class SubmitPracticeAnswerCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : ICommandHandler<SubmitPracticeAnswerCommand, Result<PracticeAnswerResultDto>>
{
    public async Task<Result<PracticeAnswerResultDto>> Handle(
        SubmitPracticeAnswerCommand request,
        CancellationToken ct
    )
    {
        var userId = currentUserService.UserId;

        if (userId is null)
            return Result<PracticeAnswerResultDto>.Unauthorized();

        // Only questions from the student's own closed quizzes.
        var allowed = await unitOfWork
            .GetOrCreateRepository<AttemptAnswer, int>()
            .AnyAsync(
                new PracticeAccessSpec(
                    userId.Value,
                    request.QuestionId,
                    DateTimeOffset.UtcNow
                ),
                ct
            );

        if (!allowed)
            return Result<PracticeAnswerResultDto>.NotFound();

        var choices = await unitOfWork
            .GetOrCreateRepository<Choice, int>()
            .ListAsync(
                new PracticeChoicesSpec([request.QuestionId]),
                ct
            );

        var correct = choices.FirstOrDefault(c => c.IsCorrect);

        if (correct is null || choices.All(c => c.ChoiceId != request.ChoiceId))
            return Result<PracticeAnswerResultDto>.Error("الاختيار غير صحيح");

        return Result.Success(
            new PracticeAnswerResultDto(
                correct.ChoiceId == request.ChoiceId,
                correct.ChoiceId)
        );
    }
}
