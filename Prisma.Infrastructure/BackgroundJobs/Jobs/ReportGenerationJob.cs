using Hangfire;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Abstractions.BackgroundJobs;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Students;

namespace Prisma.Infrastructure.BackgroundJobs.Jobs;

public class ReportGenerationJob(
    IServiceProvider serviceProvider)
    : IReportGenerationJob
{
    [Queue(JobQueues.Reports)]
    [AutomaticRetry(Attempts = 3)]
    [JobDisplayName("Student Report Generation")]
    public async Task GenerateWeekly(CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();

        var workflow = scope.ServiceProvider.GetKeyedService<Workflow>("Report-Generator");

        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var repo = uow.GetOrCreateRepository<Student, Guid>();

        var studentIds = await repo.ListAsync(new ActiveStudentIds(), cancellationToken);
        var checkpointManager = CheckpointManager.CreateInMemory();

        foreach (var studentId in studentIds)
        {
            var result = await InProcessExecution.RunAsync(workflow,
                studentId, checkpointManager, cancellationToken: cancellationToken);

            foreach (var evt in result.NewEvents)
            {
                if (evt is WorkflowOutputEvent outputEvt)
                {
                    Console.WriteLine($"Final result: {outputEvt.Data}");
                }
            }
        }
    }
}