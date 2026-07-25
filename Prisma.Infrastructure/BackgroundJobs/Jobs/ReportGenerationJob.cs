using Hangfire;
using MediatR;
using Prisma.Application.Abstractions.BackgroundJobs;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Reports.Commands.GenerateWeeklyReport;

namespace Prisma.Infrastructure.BackgroundJobs.Jobs;

public class ReportGenerationJob(ISender sender) : IReportGenerationJob
{
    public async Task GenerateWeekly(CancellationToken cancellationToken = default)
    {
        await sender.Send(new GenerateWeeklyReportCommand(), cancellationToken);
    }
}