using Hangfire;
using MediatR;
using Prisma.Application.Abstractions.BackgroundJobs;
using Prisma.Application.Features.Quizzes.Commands.FinalizeExpiredAttempts;

namespace Prisma.Infrastructure.BackgroundJobs.Jobs;

public class QuizAttemptFinalizationJob(ISender sender) : IQuizAttemptFinalizationJob
{
    [DisableConcurrentExecution(timeoutInSeconds: 5)]
    [AutomaticRetry(Attempts = 0)]
    public async Task FinalizeExpiredAttempts(CancellationToken cancellationToken = default)
    {
        await sender.Send(new FinalizeExpiredQuizAttemptsCommand(), cancellationToken);
    }
}