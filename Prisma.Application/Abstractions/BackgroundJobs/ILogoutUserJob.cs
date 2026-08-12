using Hangfire;
using Prisma.Application.Common.Constants;

namespace Prisma.Application.Abstractions.BackgroundJobs;

public interface ILogoutUserJob
{
    [Queue(JobQueues.AuthCleanup)]
    [AutomaticRetry(Attempts = 2)]
    [JobDisplayName("Clean User Tokens in Db to Respond Fast")]
    Task Clean(string? accessToken);
}