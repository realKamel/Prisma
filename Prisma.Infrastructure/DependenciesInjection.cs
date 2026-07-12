using System.ClientModel;
using System.Diagnostics.CodeAnalysis;
using Amazon.S3;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Agents.AI.DurableTask;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI;
using Prisma.Application.Abstractions.Ai;
using Prisma.Application.Abstractions.BackgroundJobs;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Infrastructure.AgenticWorkflows.ReportGeneratorWorkflow;
using Prisma.Infrastructure.AgenticWorkflows.WrittenQuestionGradingWorkflow;
using Prisma.Infrastructure.Ai;
using Prisma.Infrastructure.BackgroundJobs;
using Prisma.Infrastructure.BackgroundJobs.Jobs;
using Prisma.Infrastructure.Identity;
using Prisma.Infrastructure.Persistence;
using Prisma.Infrastructure.Persistence.Interceptors;
using Prisma.Infrastructure.Persistence.Repositories;
using Prisma.Infrastructure.Services;
using Prisma.Infrastructure.Services.Auth;
using Prisma.Infrastructure.Services.DataSeeding;
using Prisma.Infrastructure.Services.EmailService;
using Prisma.Infrastructure.Services.PaymentService;
using Prisma.Infrastructure.Services.StorageService;

namespace Prisma.Infrastructure;

public static class DependenciesInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddPersistenceConfig(configuration, environment);

        services.AddIdentityWithConfig(configuration);

        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, EmailService>();

        services.AddSingleton<IPdfTextExtractor, PdfTextExtractor>();
        services.AddSingleton<IOpenAiExamExtractor, OpenAiExamExtractor>();
        services.AddSingleton<IExtractionJobQueue, ExtractionJobQueue>();
        // services.AddScoped<IFileService, FileService>();

        services.Configure<PaymobSettings>(configuration.GetSection("PaymobSettings"));

        services.AddHttpClient<PaymobCardService>();
        services.AddHttpClient<PaymobFawryService>();

        services.AddKeyedScoped<IPaymentService, PaymobCardService>("card");
        services.AddKeyedScoped<IPaymentService, PaymobFawryService>("fawry");

        var storageConfig = configuration.GetSection("Storage");

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var config = new AmazonS3Config
            {
                ServiceURL = storageConfig["ServiceUrl"],
                ForcePathStyle = bool.Parse(storageConfig["ForcePathStyle"]!)
            };

            return new AmazonS3Client(
                storageConfig["AccessKey"],
                storageConfig["SecretKey"],
                config
            );
        });

        services.AddScoped<IStorageService, S3StorageService>();
        services.AddScoped<IVideoStorageService, MuxVideoStorageService>();
        services.AddScoped<IMuxTokenService, MuxTokenService>();

        // services.AddHostedService<StorageBucketPolicyInitializer>();

        //services.AddStackExchangeRedisCache(option =>
        //{
        //    option.Configuration = configuration.GetConnectionString("Redis");
        //});

        //services.AddDataProtection()
        //    .PersistKeysToStackExchangeRedis(
        //        ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")),
        //        "DataProtection-Keys");

        services.AddBackgroundJobsAndHangfireWithConfig(configuration);

#pragma warning disable MEAI001
        services.AddAiIntegrationServices(configuration);
#pragma warning restore MEAI001
    }

    private static void AddBackgroundJobsAndHangfireWithConfig(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options =>
            {
                options.UseNpgsqlConnection(configuration.GetConnectionString("DefaultSqlConnection"));
            }));

        // Start background job server (required for processing)
        services.AddHangfireServer(options =>
        {
            options.ServerName = $"prisma-api-{Environment.MachineName}";
            // Process all queues
            options.Queues = new[]
            {
                JobQueues.Default,
                JobQueues.Reports,
                JobQueues.VideoProcessing,
                JobQueues.AuthCleanup,
            };
            options.WorkerCount = 5;
            options.SchedulePollingInterval = TimeSpan.FromSeconds(15);
        });

        services.AddSingleton<IBackgroundJobService, HangfireBackgroundJobService>();

        // Register job classes (Hangfire needs them for DI)
        //services.AddScoped<IVideoProcessingJob, VideoProcessingJob>();
        services.AddScoped<IReportGenerationJob, ReportGenerationJob>();
    }

    private static void AddPersistenceConfig(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        // services.AddDbContextFactory<AppDbContext>(options =>
        //     options.UseNpgsql(configuration.GetConnectionString("DefaultSqlConnection"),
        //         npgSqlOptions => npgSqlOptions.UseVector()));

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultSqlConnection"), npgSqlOptions =>
            {
                // npgsqlOptions.EnableRetryOnFailure(
                //     maxRetryCount: 5,
                //     maxRetryDelay: TimeSpan.FromSeconds(10),
                //     errorCodesToAdd: null);
                npgSqlOptions.UseVector();
            });

            options.AddInterceptors(
                serviceProvider.GetRequiredService<AuditInterceptor>(),
                serviceProvider.GetRequiredService<AuditLogInterceptor>());

            if (!environment.IsDevelopment())
            {
                return;
            }

            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });
        services.AddIdentityCore<User>(options =>
            {
                //options.User.RequireUniqueEmail = true;
                if (environment.IsDevelopment())
                {
                    options.Password.RequiredLength = 4;
                    options.Password.RequireDigit = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                }
                else
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;

                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                }
            })
            .AddRoles<Role>()
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

        services.AddScoped<AuditInterceptor>();
        services.AddScoped<AuditLogInterceptor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDataSeeder, DataSeeder>();
    }

    private static void AddIdentityWithConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
    }

    [Experimental("MEAI001")]
    private static void AddAiIntegrationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var openAiConfig = configuration.GetSection("OpenAI");
        // Console.WriteLine(
        //     $"OpenAI Config: {openAiConfig["ApiKey"]}, {openAiConfig["FastChatModel"]},
        // {openAiConfig["ReasoningModel"]}, {openAiConfig["EmbeddingModel"]}, {openAiConfig["SpeechModel"]}");
        // var openAiClient = new OpenAIClient(openAiConfig["ApiKey"]!);
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri("https://models.github.ai/inference")
        };

        var openAiClient = new OpenAIClient(new ApiKeyCredential(openAiConfig["ApiKey"]!), options);

        services.AddKeyedChatClient(AIType.FastChat,
            openAiClient.GetChatClient(openAiConfig["FastChatModel"]!).AsIChatClient());

        services.AddKeyedChatClient(AIType.Reasoning,
            openAiClient.GetChatClient(openAiConfig["ReasoningModel"]!).AsIChatClient());

        services.AddKeyedEmbeddingGenerator(AIType.Embedding,
            openAiClient.GetEmbeddingClient(openAiConfig["EmbeddingModel"]!).AsIEmbeddingGenerator());

        services.AddKeyedSpeechToTextClient(AIType.SpeechToText,
            openAiClient.GetAudioClient(openAiConfig["SpeechModel"]!).AsISpeechToTextClient());


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

        services.AddSingleton<GetStudentStatusExecutor>();
        services.AddSingleton<NarrativeGenerationExecutor>();
        services.AddSingleton<GetWrittenQuestionExecutor>();
        services.AddSingleton<GradingQuestionExecutor>();
    }

    public static void AddAiAgents(this IHostApplicationBuilder app)
    {
        app.AddAIAgent(AIAgentRole.ChatAgent.DefaultAgent,
                AIAgentRole.ChatAgent.DefaultAgentInstructions, AIType.FastChat)
            .WithInMemorySessionStore();

        app.AddAIAgent(AIAgentRole.ChatAgent.GradingAgent,
            AIAgentRole.ChatAgent.GradingAgentInstructions, AIType.Reasoning);

        // app.AddAIAgent(AIAgentRole.ChatAgent.KnowledgeRagChatAgent,
        //         AIAgentRole.ChatAgent.KnowledgeRagChatAgentInstructions, AIType.FastChat)
        //     .WithInMemorySessionStore();


        app.AddAIAgent(AIAgentRole.ChatAgent.KnowledgeRagChatAgent,
            ((provider, key) =>
            {
                var chatClient = provider.GetRequiredKeyedService<IChatClient>(AIType.FastChat);
                var ragFunctions = provider.GetRequiredService<AgentRagTools>();

                return chatClient.AsAIAgent(
                    AIAgentRole.ChatAgent.KnowledgeRagChatAgentInstructions,
                    name: key,
                    tools:
                    [
                        AIFunctionFactory.Create(ragFunctions.SearchLessonsContentAsync),
                        AIFunctionFactory.Create(ragFunctions.SearchLessonContentAsync)
                    ]
                );
            })).WithInMemorySessionStore();
    }

    public static void AddWorkflows(this IHostApplicationBuilder app)
    {
        app.AddWorkflow("Report-Generator", (sp, key) =>
        {
            var processor = sp.GetRequiredService<GetStudentStatusExecutor>();
            var narrativeGenerator = sp.GetRequiredService<NarrativeGenerationExecutor>();
            return new WorkflowBuilder(processor)
                .WithName(key)
                .AddEdge(processor, narrativeGenerator)
                .Build();
        });

        app.AddWorkflow("Written-Quesions-Grades", (sp, key) =>
        {
            var processor = sp.GetRequiredService<GetWrittenQuestionExecutor>();
            var grader = sp.GetRequiredService<GradingQuestionExecutor>();

            return new WorkflowBuilder(processor)
                .AddEdge(processor, grader)
                .WithName(key)
                .Build();
        });
    }
}