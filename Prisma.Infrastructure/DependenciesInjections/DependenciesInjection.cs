using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Interfaces;
using Prisma.Infrastructure.Localization;
using Prisma.Infrastructure.Persistence;
using Prisma.Infrastructure.Services;

namespace Prisma.Infrastructure.DependenciesInjections;

public static partial class DependenciesInjection
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

        services.AddMuxStreaming();

        // services.AddHostedService<StorageBucketPolicyInitializer>();

        services.AddBackgroundJobsAndHangfireWithConfig(configuration);

        services.AddAiIntegrationServices(configuration);

        services.AddScoped<ISummarizationServices, SummarizationServices>();
        services.AddScoped<ITextEmbeddingProcessor, TextEmbeddingProcessor>();

        services.AddCacheServices(configuration);
        services.AddLocalizationServices();
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
}
