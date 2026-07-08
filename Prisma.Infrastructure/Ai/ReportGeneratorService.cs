using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Abstractions.Ai;
using Prisma.Application.Common.Constants;

namespace Prisma.Infrastructure.Ai;

public sealed class ReportGeneratorService(
    [FromKeyedServices(AIAgentRole.ChatAgent.DefaultAgent)]
    AIAgent agent)
    : IReportGenerator
{
    public async Task<string> GenerateReportAsync(ReportRequest request, CancellationToken ct)
    {
        var prompt = BuildPrompt(request);

        var response = await agent.RunAsync(prompt, cancellationToken: ct);

        return response.Text;
    }

    private static string BuildPrompt(ReportRequest request)
    {
        var metricsBlock = string.Join('\n', request.Metrics.Select(m =>
            m.Trend is null ? $"- {m.Label}: {m.Value}" : $"- {m.Label}: {m.Value} ({m.Trend})"));

        return $"""
                Generate a {request.Type} for {request.RecipientName},
                covering {request.PeriodStart:d} to {request.PeriodEnd:d}.

                Metrics:
                {metricsBlock}

                {request.AdditionalContext}
                """;
    }
}