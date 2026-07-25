using Hangfire;
using Prisma.Application.Common.Constants;

namespace Prisma.Application.Abstractions.BackgroundJobs;

public interface IReportGenerationJob
{
    [Queue(JobQueues.Reports)]
    [AutomaticRetry(Attempts = 3)]
    [JobDisplayName("Student Report Generation")]
    Task GenerateWeekly(CancellationToken cancellationToken = default);
}