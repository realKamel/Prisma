using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Prisma.Application.Abstractions.Ai;

public interface IRagQuestionAnswering
{
    IAsyncEnumerable<RagAnswer> AskAsync(string question, string? existingThreadState, CancellationToken ct);
    Task<List<ChatMessage>> GetChatMessagesAsync(string? existingThreadState, CancellationToken ct);
    List<ChatMessage> GetChatMessagesAsync(AgentSession? agentSession);
}

public sealed record RagAnswer(string Text, string? ThreadState, AgentSession session);