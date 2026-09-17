
using MediatR;
using Microsoft.Extensions.Logging;
using Prisma.Application.Features.Quizzes.Common;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Commands.FinalizeExpiredAttempts;

public class FinalizeExpiredQuizAttemptsCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<FinalizeExpiredQuizAttemptsCommandHandler> logger)
    : IRequestHandler<FinalizeExpiredQuizAttemptsCommand>
{
    public async Task Handle(FinalizeExpiredQuizAttemptsCommand request, CancellationToken ct)
    {
        var attemptRepo = unitOfWork.GetOrCreateRepository<QuizAttempt, int>();
        var expired = await attemptRepo.ListAsync(new ExpiredInProgressAttemptsSpecification(), ct);

        if (expired.Count == 0)
        {
            return;
        }

        var quizRepo = unitOfWork.GetOrCreateRepository<Quiz, int>();

        foreach (var group in expired.GroupBy(a => a.QuizId))
        {
            var quiz = await quizRepo.FirstOrDefaultAsync(new QuizForFinalizationSpecification(group.Key), ct);
            if (quiz is null)
            {
                logger.LogWarning("Quiz {QuizId} not found while finalizing expired attempts", group.Key);
                continue;
            }

            foreach (var attempt in group)
            {
                await QuizFinalizer.FinalizeAttempt(attempt, quiz, unitOfWork, ct);
            }
        }
    }
}
