using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.DTOs.Ai;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Infrastructure.AgenticWorkflows.ReportGeneratorWorkflow;

public partial class NarrativeGenerationExecutor(IServiceScopeFactory serviceProvider)
    : Executor("NarrativeGenerationExecutor")
{
    [MessageHandler]
    private async ValueTask HandleAsync(StudentData message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
    }
}