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
    IUnitOfWork uow,
    [FromKeyedServices("Report-Generator")]
    Workflow workflow)
    : IReportGenerationJob
{
    [Queue(JobQueues.Reports)]
    [AutomaticRetry(Attempts = 3)]
    public async Task GenerateWeekly(CancellationToken cancellationToken = default)
    {
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