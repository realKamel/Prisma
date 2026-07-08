using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Abstractions.Ai;
using Prisma.Application.Common.Constants;

namespace Prisma.Infrastructure.Ai;

internal sealed class GradingAssistantService(
    [FromKeyedServices(AIAgentRole.ChatAgent.GradingAgent)]
    AIAgent agent)
    : IGradingAssistant
{
    public async Task<GradingSuggestionResult> SuggestGradeAsync(
        string studentAnswer, string modelAnswer, int maxScore, CancellationToken ct)
    {
        var prompt = $"Model answer: {modelAnswer}\nStudent answer: {studentAnswer}\nMax score: {maxScore}";
        var response = await agent.RunAsync<GradingSuggestionResult>(prompt, cancellationToken: ct);
        return response.Result;
    }
}