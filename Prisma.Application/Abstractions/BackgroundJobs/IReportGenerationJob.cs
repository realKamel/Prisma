using Hangfire;
using Prisma.Application.Common.Constants;

namespace Prisma.Application.Abstractions.BackgroundJobs;

public interface IReportGenerationJob
{
    [Queue(JobQueues.Reporting)]
    [AutomaticRetry(Attempts = 3)]
    [JobDisplayName("Reports Generator")]
    Task GenerateWeekly(CancellationToken cancellationToken = default);
}
