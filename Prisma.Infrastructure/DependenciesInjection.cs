using System.ClientModel;
using Amazon.S3;
using Groq.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
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
using Prisma.Domain.Repositories;
using Prisma.Infrastructure.AgenticWorkflows.ReportGeneratorWorkflow;
using Prisma.Infrastructure.AgenticWorkflows.WrittenQuestionGradingWorkflow;
using Prisma.Infrastructure.Ai;
using Prisma.Infrastructure.BackgroundJobs;
using Prisma.Infrastructure.BackgroundJobs.Jobs;
using Prisma.Infrastructure.Identity;
using Prisma.Infrastructure.Localization;
using Prisma.Infrastructure.Persistence;
using Prisma.Infrastructure.Persistence.Interceptors;
using Prisma.Infrastructure.Persistence.Repositories;
using Prisma.Infrastructure.Services;
using Prisma.Infrastructure.Services.Auth;
using Prisma.Infrastructure.Services.DataSeeding;
using Prisma.Infrastructure.Services.EmailService;
using Prisma.Infrastructure.Services.PaymentService;
using Prisma.Infrastructure.Services.StorageService;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace Prisma.Infrastructure;

public static class DependenciesInjection
{
    public static void AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        services.AddPersistenceConfig(configuration, environment);

        services.AddInfrastructureHealthChecks(configuration);

        services.AddIdentityWithConfig(environment, configuration);

        services.AddEmailServices(configuration);

        services.AddSingleton<IPdfTextExtractor, PdfTextExtractor>();
        services.AddSingleton<IOpenAiExamExtractor, OpenAiExamExtractor>();
        services.AddSingleton<IExtractionJobQueue, ExtractionJobQueue>();

        services.AddPaymentServices(configuration);

        services.AddObjectStorageServices(configuration);

        services.AddScoped<IVideoStorageService, MuxVideoStorageService>();
        services.AddScoped<IMuxTokenService, MuxTokenService>();

        // services.AddHostedService<StorageBucketPolicyInitializer>();

        services.AddBackgroundJobsAndHangfireWithConfig(configuration);

        services.AddAiIntegrationServices(configuration);

        services.AddScoped<ISummarizationServices, SummarizationServices>();
        services.AddScoped<ITextEmbeddingProcessor, TextEmbeddingProcessor>();

        services.AddCacheServices(configuration);
        services.AddLocalizationServices();
    }

    private static void AddBackgroundJobsAndHangfireWithConfig(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHangfire(config =>
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(
                        configuration.GetConnectionString("DefaultSqlConnection")
                    );
                })
        );

        // Start background job server (required for processing)
        services.AddHangfireServer(options =>
        {
            options.ServerName = $"prisma-api-{Environment.MachineName}";
            // Process all queues
            options.Queues =
            [
                JobQueues.Default,
                JobQueues.Reports,
                JobQueues.VideoProcessing,
                JobQueues.AuthCleanup,
            ];
            options.WorkerCount = 3;
            options.SchedulePollingInterval = TimeSpan.FromSeconds(15);
        });

        services.AddSingleton<IBackgroundJobService, HangfireBackgroundJobService>();

        // Register job classes (Hangfire needs them for DI)
        //services.AddScoped<IVideoProcessingJob, VideoProcessingJob>();
        services.AddScoped<IReportGenerationJob, ReportGenerationJob>();
        services.AddScoped<ILessonTranscriptAndSummarizationJob, LessonTranscriptAndSummaryJob>();
        services.AddHttpClient<IAudioStreamingService, MuxHttpClient>(client =>
        {
            client.BaseAddress = new Uri("https://stream.mux.com");
            client.DefaultRequestHeaders.Accept.Clear();
        });
        services.AddScoped<ILogoutUserJob, CleanUpAuth>();
    }

    private static void AddPersistenceConfig(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        // services.AddDbContextFactory<AppDbContext>(options =>
        //     options.UseNpgsql(configuration.GetConnectionString("DefaultSqlConnection"),
        //         npgSqlOptions => npgSqlOptions.UseVector()));

        services.AddDbContext<AppDbContext>(
            (serviceProvider, options) =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultSqlConnection"),
                    npgSqlOptions =>
                    {
                        npgSqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorCodesToAdd: null
                        );
                        npgSqlOptions.UseVector();
                    }
                );

                options.AddInterceptors(
                    serviceProvider.GetRequiredService<AuditInterceptor>(),
                    serviceProvider.GetRequiredService<AuditLogInterceptor>()
                );

                if (environment.IsDevelopment())
                {
                    options.EnableDetailedErrors();

                    // ONLY uncomment the line below when you actively need to see SQL parameter values
                    // to debug a specific query. NEVER leave it uncommented in Production.
                    // options.EnableSensitiveDataLogging();
                }
            }
        );

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

        services.AddScoped<AuditInterceptor>();
        services.AddScoped<AuditLogInterceptor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDataSeeder, DataSeeder>();

        //Custom Repositories
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
    }

    private static void AddIdentityWithConfig(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration
    )
    {
        services.AddHttpContextAccessor();

        services
            .AddIdentityCore<User>(options =>
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
            .AddRoles<Domain.Entities.UserAggregate.Role>()
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<AppDbContext>();

        //services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services
            .AddOptions<JwtSettingsOptions>()
            .Bind(configuration.GetSection(JwtSettingsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
    }

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

    private static void AddInfrastructureHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionStrings = configuration
            .GetSection(ConnectionStringsOptions.SectionName)
            .Get<ConnectionStringsOptions>();

        ArgumentNullException.ThrowIfNull(connectionStrings);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            connectionStrings.DefaultSqlConnection,
            nameof(connectionStrings.DefaultSqlConnection)
        );

        ArgumentException.ThrowIfNullOrWhiteSpace(
            connectionStrings.Valkey,
            nameof(connectionStrings.Valkey)
        );

        services
            .AddHealthChecks()
            .AddNpgSql(
                connectionString: connectionStrings.DefaultSqlConnection,
                name: "PostgreSQL",
                tags: ["db", "sql", "postgresql", "ready"]
            )
            .AddRedis(connectionStrings.Valkey, name: "Valkey", tags: ["cache", "valkey", "ready"])
            .AddHangfire(
                setup =>
                {
                    setup.MaximumJobsFailed = 5; // Default to 5 if not configured
                    setup.MinimumAvailableServers = 1; // Default to 1
                },
                name: "hangfire",
                tags: ["jobs", "hangfire", "ready"]
            );
    }

    private static void AddLocalizationServices(this IServiceCollection services)
    {
        services.AddLocalization();
        services.AddTransient<IAppLocalizer, AppLocalizer>();
    }

    private static void AddCacheServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionStrings = configuration
            .GetSection(ConnectionStringsOptions.SectionName)
            .Get<ConnectionStringsOptions>();

        ArgumentNullException.ThrowIfNull(connectionStrings);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            connectionStrings.Valkey,
            nameof(connectionStrings.Valkey)
        );

        services
            .AddDataProtection()
            .PersistKeysToStackExchangeRedis(
                ConnectionMultiplexer.Connect(connectionStrings.Valkey),
                "DataProtection-Keys"
            );

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionStrings.Valkey;
        });

        services
            .AddFusionCache("prisma_cache")
            .WithDefaultEntryOptions(options =>
            {
                // General Cache Duration
                options.Duration = TimeSpan.FromMinutes(10);

                // A. Fail-Safe: If DB crashes under load, we serve stale exam data
                // up to 6 (will change) hours instead of throwing an HTTP 500 error to students.
                options.IsFailSafeEnabled = true;
                options.FailSafeMaxDuration = TimeSpan.FromHours(24);
                options.FailSafeThrottleDuration = TimeSpan.FromSeconds(30);

                // B. Soft Timeout: If DB takes longer than 150ms during a rush,
                // abort waiting and instantly entry from the cached payload.
                options.FactorySoftTimeout = TimeSpan.FromMilliseconds(150);

                // C. Hard Timeout: Never allow a DB call to block an API thread
                // for longer than 2 seconds.
                options.FactoryHardTimeout = TimeSpan.FromSeconds(2);

                // Dynamic Jittering: random extra seconds to expiration times.
                // Prevents thousands of cache entries from expiring simultaneously.
                options.JitterMaxDuration = TimeSpan.FromSeconds(30);
            })
            // Serializer for L2 Valkey Cache
            .WithSerializer(new FusionCacheSystemTextJsonSerializer())
            .WithDistributedCache(sp => sp.GetRequiredService<IDistributedCache>())
            // Backplane: Syncs all API nodes so no server returns old data
            .WithBackplane(
                new RedisBackplane(
                    new RedisBackplaneOptions { Configuration = connectionStrings.Valkey }
                )
            )
            .AsHybridCache();
    }

    private static void AddEmailServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        services.AddScoped<IEmailService, EmailService>();
    }

    private static void AddPaymentServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<PaymobSettings>(configuration.GetSection("PaymobSettings"));

        services.AddHttpClient<PaymobCardService>();
        services.AddHttpClient<PaymobFawryService>();

        services.AddKeyedScoped<IPaymentService, PaymobCardService>("card");
        services.AddKeyedScoped<IPaymentService, PaymobFawryService>("fawry");
    }

    private static void AddObjectStorageServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var storageConfig = configuration
            .GetSection(ObjectStorageOptions.SectionName)
            .Get<ObjectStorageOptions>();

        ArgumentNullException.ThrowIfNull(storageConfig);

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var config = new AmazonS3Config
            {
                ServiceURL = storageConfig.ServiceUrl,
                ForcePathStyle = storageConfig.ForcePathStyle,
            };

            return new AmazonS3Client(storageConfig.AccessKey, storageConfig.SecretKey, config);
        });

        services.AddScoped<IStorageService, S3StorageService>();
    }
}
