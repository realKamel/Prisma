
using MediatR;
using Microsoft.Extensions.Logging;
using Prisma.Application.Features.Quizzes.Common;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Commands.FinalizeExpiredAttempts;

public class FinalizeExpiredQuizAttemptsCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<FinalizeExpiredQuizAttemptsCommandHandler> logger)
    : IRequestHandler<FinalizeExpiredQuizAttemptsCommand>
{
    private const int BatchSize = 100;

    public async Task Handle(FinalizeExpiredQuizAttemptsCommand request, CancellationToken ct)
    {
        var attemptRepo = unitOfWork.GetOrCreateRepository<QuizAttempt, int>();
        var lastId = 0;
        var finalized = 0;

        while (!ct.IsCancellationRequested)
        {
            var batch = await attemptRepo.ListAsync(
                new ExpiredInProgressAttemptsSpecification(lastId, BatchSize, TimeSpan.FromSeconds(QuizConstants.GracePeriodSeconds)), ct);

            if (batch.Count == 0) break;
            lastId = batch.Max(a => a.Id);

            try
            {
                finalized += await FinalizeBatch(batch, ct);
            }
            catch (ConcurrencyConflictException)
            {
                // طالب سلّم في نفس اللحظة. الـ batch هيتعاد في الدورة الجاية
                // والـ attempt اللي اتسلّم مش هيرجع لأنه مبقاش InProgress
                logger.LogInformation("Concurrency conflict while finalizing batch ending at attempt {LastId}; will retry next run", lastId);
                unitOfWork.ClearChangeTracker();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // الـ batch ده هيتعاد في الدورة الجاية، وبنكمل على اللي بعده
                logger.LogError(ex, "Failed to finalize expired attempts batch ending at attempt {LastId}", lastId);
                unitOfWork.ClearChangeTracker();
            }

            if (batch.Count < BatchSize) break;
        }

        if (finalized > 0)
            logger.LogInformation("Auto-finalized {Count} expired quiz attempts", finalized);
    }

    private async Task<int> FinalizeBatch(IReadOnlyList<QuizAttempt> batch, CancellationToken ct)
    {
        // Query واحدة لكل الـ quizzes في الـ batch
        var quizIds = batch.Select(a => a.QuizId).Distinct().ToList();
        var quizRepo = unitOfWork.GetOrCreateRepository<Quiz, int>();
        var quizzes = (await quizRepo.ListAsync(new QuizzesForFinalizationSpecification(quizIds), ct))
            .ToDictionary(q => q.Id);

        var lessonCompletions = new List<(Guid StudentId, int LessonId)>();
        var count = 0;

        foreach (var attempt in batch)
        {
            if (!quizzes.TryGetValue(attempt.QuizId, out var quiz))
            {
                logger.LogWarning("Quiz {QuizId} not found for expired attempt {AttemptId}", attempt.QuizId, attempt.Id);
                continue;
            }

            var fullyGraded = QuizFinalizer.ApplyFinalization(attempt, quiz, submittedAt: attempt.Deadline);

            if (fullyGraded && quiz.Scope == QuizScope.LessonQuiz && quiz.LessonId.HasValue)
                lessonCompletions.Add((attempt.StudentId, quiz.LessonId.Value));

            count++;
        }

        await QuizFinalizer.CompleteLessonsInBulk(lessonCompletions, unitOfWork, ct);
        await unitOfWork.SaveChangesAsync(ct); 
        return count;
    }
}