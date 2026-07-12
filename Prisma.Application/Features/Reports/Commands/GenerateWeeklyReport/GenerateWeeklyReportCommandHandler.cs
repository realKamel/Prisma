using MediatR;
using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Common.Constants;

namespace Prisma.Application.Features.Reports.Commands.GenerateWeeklyReport;

internal class GenerateWeeklyReportCommandHandler
    : IRequestHandler<GenerateWeeklyReportCommand>
{
    public Task Handle(GenerateWeeklyReportCommand request, CancellationToken cancellationToken)
    {
        // aiAgent.
        return Task.CompletedTask;
    }
}