using System.ClientModel;
using Groq.Extensions.DependencyInjection;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI;
using Prisma.Application.Abstractions.Ai;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Interfaces;
using Prisma.Infrastructure.AgenticWorkflows.ReportGeneratorWorkflow;
using Prisma.Infrastructure.AgenticWorkflows.WrittenQuestionGradingWorkflow;
using Prisma.Infrastructure.Ai;
using Prisma.Infrastructure.Persistence.Repositories;

namespace Prisma.Infrastructure.DependenciesInjections;

public static partial class DependenciesInjection
{
    private static void AddAiIntegrationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var openAiConfig = configuration.GetSection("OpenAI");
        // Console.WriteLine(
        //     $"OpenAI Config: {openAiConfig["ApiKey"]}, {openAiConfig["FastChatModel"]},
        // {openAiConfig["ReasoningModel"]}, {openAiConfig["EmbeddingModel"]}, {openAiConfig["SpeechModel"]}");
        // var openAiClient = new OpenAIClient(openAiConfig["ApiKey"]!);
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri("https://models.github.ai/inference"),
        };

        var openAiClient = new OpenAIClient(new ApiKeyCredential(openAiConfig["ApiKey"]!), options);

        services.AddKeyedChatClient(
            AIType.FastChat,
            openAiClient.GetChatClient(openAiConfig["FastChatModel"]!).AsIChatClient()
        );

        services.AddKeyedChatClient(
            AIType.Reasoning,
            openAiClient.GetChatClient(openAiConfig["ReasoningModel"]!).AsIChatClient()
        );

        services.AddKeyedEmbeddingGenerator(
            AIType.Embedding,
            openAiClient.GetEmbeddingClient(openAiConfig["EmbeddingModel"]!).AsIEmbeddingGenerator()
        );

        // services.AddKeyedSpeechToTextClient(AIType.SpeechToText,
        //     openAiClient.GetAudioClient(openAiConfig["SpeechModel"]!).AsISpeechToTextClient());

        services.AddScoped<IRagQuestionAnswering, RagQuestionAnsweringService>();

        services.AddScoped<IGradingAssistant, GradingAssistantService>();
        services.AddScoped<IReportGenerator, ReportGeneratorService>();
        services.AddScoped<ITranscriptionService, TranscriptionService>();

        services.AddSingleton<IEmbeddingService, EmbeddingService>();
        services.AddSingleton<IVectorSearchRepository, VectorSearchRepository>();

        services.AddSingleton<AgentRagTools>();

        // services.AddSingleton<ChatHistoryProvider>(sp =>
        //     new PostgresChatHistoryProvider(
        //         sp.GetRequiredService<IDbContextFactory<AppDbContext>>(),
        //         maxMessagesToLoad: 40));

        // services.AddSingleton<GetStudentStatusExecutor>();
        // services.AddSingleton<NarrativeGenerationExecutor>();
        // services.AddSingleton<GetWrittenQuestionExecutor>();
        // services.AddSingleton<GradingQuestionExecutor>();
    }

    public static void AddAiAgents(this IHostApplicationBuilder app, IConfiguration configuration)
    {
        app.AddAIAgent(
                AIAgentRole.ChatAgent.DefaultAgent,
                AIAgentRole.ChatAgent.DefaultAgentInstructions,
                AIType.FastChat
            )
            .WithInMemorySessionStore();

        app.AddAIAgent(
            AIAgentRole.ChatAgent.GradingAgent,
            AIAgentRole.ChatAgent.GradingAgentInstructions,
            AIType.Reasoning
        );

        // app.AddAIAgent(AIAgentRole.ChatAgent.KnowledgeRagChatAgent,
        //         AIAgentRole.ChatAgent.KnowledgeRagChatAgentInstructions, AIType.FastChat)
        //     .WithInMemorySessionStore();

        app.AddAIAgent(
                AIAgentRole.ChatAgent.KnowledgeRagChatAgent,
                (
                    (provider, key) =>
                    {
                        var chatClient = provider.GetRequiredKeyedService<IChatClient>(
                            AIType.FastChat
                        );
                        var ragFunctions = provider.GetRequiredService<AgentRagTools>();

                        return chatClient.AsAIAgent(
                            AIAgentRole.ChatAgent.KnowledgeRagChatAgentInstructions,
                            name: key,
                            tools:
                            [
                                AIFunctionFactory.Create(ragFunctions.SearchLessonsContentAsync),
                                AIFunctionFactory.Create(ragFunctions.SearchLessonContentAsync),
                            ]
                        );
                    }
                )
            )
            .WithInMemorySessionStore();

        app.AddAIAgent(
            AIAgentRole.ChatAgent.ReportGeneratorAgent,
            AIAgentRole.ChatAgent.ReportGeneratorAgentInstructions,
            AIType.Reasoning
        );

        app.AddGroqApiServices(options =>
        {
            options.ApiKey =
                configuration?.GetSection("Groq")["ApiKey"]
                ?? throw new ArgumentNullException("Groq ApiKey is null");

            options.Timeout = TimeSpan.FromSeconds(100);
            options.MaxRetries = 3;
        });
    }

    public static void AddWorkflows(this IHostApplicationBuilder app)
    {
        app.AddWorkflow(
            "Report-Generator",
            (sp, key) =>
            {
                var processor = sp.GetRequiredService<GetStudentStatusExecutor>();
                var narrativeGenerator = sp.GetRequiredService<NarrativeGenerationExecutor>();
                return new WorkflowBuilder(processor)
                    .WithName(key)
                    .AddEdge(processor, narrativeGenerator)
                    .Build();
            }
        );

        app.AddWorkflow(
            "Written-Quesions-Grades",
            (sp, key) =>
            {
                var processor = sp.GetRequiredService<GetWrittenQuestionExecutor>();
                var grader = sp.GetRequiredService<GradingQuestionExecutor>();

                return new WorkflowBuilder(processor)
                    .AddEdge(processor, grader)
                    .WithName(key)
                    .Build();
            }
        );
    }
}
