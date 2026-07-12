namespace Prisma.Application.Abstractions.BackgroundJobs;

public interface IReportGenerationJob
{
    Task GenerateWeekly(CancellationToken cancellationToken = default);
}