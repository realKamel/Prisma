using Hangfire;
using MediatR;
using Prisma.Application.Abstractions.BackgroundJobs;
using Prisma.Application.Common.Constants.BackgroundJobs;
using Prisma.Application.Features.Reports.Commands.GenerateWeeklyReport;

namespace Prisma.Infrastructure.BackgroundJobs.Jobs;

public class ReportGenerationJob(IMediator mediator) : IReportGenerationJob
{
    [Queue(JobQueues.Reports)]
    [AutomaticRetry(Attempts = 3)]
    public async Task GenerateWeekly()
    {
        await mediator.Send(new GenerateWeeklyReportCommand());
    }
}