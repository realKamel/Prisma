using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Abstractions.Ai;
using Prisma.Application.Common.Constants;

namespace Prisma.Infrastructure.Ai;

public class RagQuestionAnsweringService(
    [FromKeyedServices(AIAgentRole.ChatAgent.KnowledgeRagChatAgent)]
    AIAgent agent)
    : IRagQuestionAnswering
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
    {
        AllowOutOfOrderMetadataProperties = true,
        AllowTrailingCommas = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    public async IAsyncEnumerable<RagAnswer> AskAsync(string question, string? existingThreadState,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        AgentSession agentSession;

        if (!string.IsNullOrWhiteSpace(existingThreadState))
        {
            try
            {
                using var parsed = JsonDocument.Parse(existingThreadState);

                agentSession = await agent
                    .DeserializeSessionAsync(parsed.RootElement.Clone(),
                        _jsonSerializerOptions, cancellationToken);
            }
            catch (JsonException)
            {
                // corrupted state — decide: fail the request, or fall back to a fresh session?
                agentSession = await agent.CreateSessionAsync(cancellationToken);
            }
        }
        else
        {
            agentSession = await agent.CreateSessionAsync(cancellationToken);
        }


        var response = agent
            .RunStreamingAsync(question,
                agentSession, null, cancellationToken);

        var serializedElement = await agent
            .SerializeSessionAsync(agentSession,
                _jsonSerializerOptions, cancellationToken);

        var serializedState = serializedElement.GetRawText();

        await foreach (var ragAnswer in response)
        {
            yield return new RagAnswer(ragAnswer.Text, serializedState, agentSession);
        }
    }

    public async Task<List<ChatMessage>> GetChatMessagesAsync(string? existingThreadState,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(existingThreadState)) return [];

        try
        {
            using var parsed = JsonDocument.Parse(existingThreadState);
            var provider = new InMemoryChatHistoryProvider();

            var session = await agent
                .DeserializeSessionAsync(parsed.RootElement.Clone(),
                    _jsonSerializerOptions, cancellationToken);
            return provider.GetMessages(session);
        }
        catch (JsonException)
        {
            // corrupted state — decide: fail the request, or fall back to a fresh session?
            return [];
        }
    }

    public List<ChatMessage> GetChatMessagesAsync(AgentSession? agentSession)
    {
        if (agentSession is null) return [];

        var provider = new InMemoryChatHistoryProvider();
        return provider.GetMessages(agentSession);
    }
}