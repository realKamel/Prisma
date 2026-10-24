using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.Result.AspNetCore;
using Hangfire;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Prisma.API.Filters;
using Prisma.API.Localization;
using Prisma.API.Middlewares;
using Prisma.Application;
using Prisma.Application.Abstractions.BackgroundJobs;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Common;
using Prisma.Infrastructure.BackgroundJobs.Jobs;
using Prisma.Infrastructure.DependenciesInjections;
using Prisma.Infrastructure.Persistence;
using Prisma.Infrastructure.Services.DataSeeding;
using Serilog;
using StackExchange.Redis;
using tusdotnet;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Stores;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Postgresql;
using Wolverine.Redis;

namespace Prisma.API.Extensions;

public static partial class WebAppHelper
{
    internal static void AddWebAppServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment
    )
    {
        // web api services
        services.AddSerilog(
            (sp, loggerConfiguration) =>
                loggerConfiguration
                    .ReadFrom.Configuration(configuration)
                    .ReadFrom.Services(sp)
                    .Enrich.FromLogContext()
        );

        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                ctx.ProblemDetails.Extensions.TryAdd("traceId", ctx.HttpContext.TraceIdentifier);
            };
        });

        services
            .AddControllers(options => options.AddDefaultResultConvention())
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                );
            });

        services.AddApiVersioningConfiguration();

        services.AddExceptionHandler<GlobalExceptionHandler>();

        //Application Services
        services.AddApplicationServices(configuration);

        //Infrastructure Services
        services.AddInfrastructureServices(configuration, hostEnvironment);

        // Presentation-layer bindings for real-time chat
        services.AddChatRealTimeServices();

        services.AddJwtAuthentication(configuration, hostEnvironment);
        services.AddOutputCacheServices(configuration);
        services.AddLocalizationServices();
        services.AddRateLimiterConfiguration(configuration);
        services.AddObservabilityServices();

        // Add forwarded headers BEFORE anything else that reads the request scheme
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            // Clear known networks and proxies so .NET trusts (Caddy|Traefik)'s headers inside Docker
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        //services.AddHealthChecksUI(setup =>
        // {
        //     setup.SetEvaluationTimeInSeconds(30); // How often the UI polls the /health/ready endpoint
        //     setup.AddHealthCheckEndpoint("API Health", "/health/ready");
        // }).AddInMemoryStorage();

        // services.AddOpenAIResponses();
        // services.AddOpenAIConversations();
        // services.AddDevUI();

        var connectionStrings = configuration
            .GetSection(ConnectionStringsOptions.SectionName)
            .Get<ConnectionStringsOptions>();

        ArgumentNullException.ThrowIfNull(connectionStrings);

        var multiplexer = ConnectionMultiplexer.Connect(
            new ConfigurationOptions
            {
                EndPoints = { connectionStrings.ValkeyMessaging },
                AbortOnConnectFail = false,
                ConnectRetry = 10,
                ConnectTimeout = 5000,
                SyncTimeout = 5000,
            }
        );
        services.AddKeyedSingleton<IConnectionMultiplexer>("messaging", multiplexer);
        services.AddSingleton<ITusStore>(_ => new TusDiskStore("/uploads"));
    }

    private static void AddLocalizationServices(this IServiceCollection services)
    {
        services.AddLocalization();
        services.AddTransient<IAppLocalizer, AppLocalizer>();
    }

    internal static async Task UseDataSeedingAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
        await services.SeedAppDataAsync();
    }

    internal static void UseRecurringJobs(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IBackgroundJobService jobService =
            scope.ServiceProvider.GetRequiredService<IBackgroundJobService>();

        //Every Friday at 10:00 PM
        jobService.AddOrUpdateRecurring<ReportGenerationJob>(
            JobQueues.Reports,
            x => x.GenerateWeekly(),
            Cron.Weekly(DayOfWeek.Friday, 22, 0)
        );

        // Every minute — catches quiz attempts whose time ran out without the student re-opening the page
        // jobService.AddOrUpdateRecurring<QuizAttemptFinalizationJob>(
        //     JobQueues.QuizAttemptFinalization,
        //     x => x.FinalizeExpiredAttempts(CancellationToken.None),
        //     Cron.Minutely());
    }

    internal static void UseHangfireUi(this WebApplication app)
    {
        app.UseHangfireDashboard(
            "/hangfire",
            new DashboardOptions
            {
                Authorization = [new HangfireDashboardAuthFilter()], //TODO: restrict to admins
            }
        );
    }

    internal static void MapAppHealthChecks(this WebApplication app)
    {
        // 1. Liveness Probe (Lightweight)
        // Kubernetes uses this to know if the app process is alive.
        // We exclude heavy checks (like DB), so a temporary DB blip doesn't restart the whole pod.
        app.MapHealthChecks(
            "/health/live",
            new HealthCheckOptions
            {
                Predicate = check => !check.Tags.Contains("ready"), // Runs checks WITHOUT the "ready" tag
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            }
        );

        // 2. Readiness Probe (Heavyweight)
        // Kubernetes / Load Balancers use this to know if the app can accept traffic.
        // This includes Postgres, Valkey, and Hangfire (as we tagged them with "ready").
        app.MapHealthChecks(
            "/health/ready",
            new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            }
        );
    }

    internal static void MapOpenAiResponses(this WebApplication app, IHostEnvironment environment)
    {
        app.MapOpenAIResponses();
        app.MapOpenAIConversations();
        if (environment.IsDevelopment())
        {
            // Map DevUI endpoint to /devui
            // app.MapDevUI();
        }
    }

    internal static void UseLocalization(this WebApplication app)
    {
        var supportedCultures = new[] { new CultureInfo("en-US"), new CultureInfo("ar-EG") };
        app.UseRequestLocalization(
            new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures, // For dates, numbers, currency
                SupportedUICultures = supportedCultures, // For string localizations
            }
        );
    }

    internal static void MapTusEndpoints(this WebApplication app)
    {
        app.MapTus(
            "/tus",
            httpContext =>
            {
                var store = httpContext.RequestServices.GetRequiredService<ITusStore>();

                return Task.FromResult<DefaultTusConfiguration?>(
                    new DefaultTusConfiguration
                    {
                        Store = store,
                        MetadataParsingStrategy = MetadataParsingStrategy.AllowEmptyValues,
                        UsePipelinesIfAvailable = true,
                    }
                );
            }
        );
    }

    internal static void UseWolverineWithConfiguration(
        this IHostBuilder host,
        IConfiguration configuration
    )
    {
        var connectionStrings = configuration
            .GetSection(ConnectionStringsOptions.SectionName)
            .Get<ConnectionStringsOptions>();

        ArgumentNullException.ThrowIfNull(connectionStrings);

        host.UseWolverine(options =>
        {
            // Wolverine only scans the entry assembly by default, while chat event handlers
            // and existing application handlers live in Prisma.Application.
            options.Discovery.IncludeAssembly(typeof(ApplicationAssemblyMarker).Assembly);

            // Enable EF Core transactional outbox & inbox middleware automatically
            options.UseEntityFrameworkCoreTransactions();

            // Automatically scrape and publish domain events from DbContext entities upon SaveChangesAsync
            options.PublishDomainEventsFromEntityFrameworkCore<IDomainEventEnumerable>(e =>
                e.DomainEvents
            );

            // Direct Wolverine to persist outbox messages using your database provider
            options.PersistMessagesWithPostgresql(connectionStrings.DefaultSqlConnection);

            // Enable durable outbox processing across all handlers/endpoints
            options.Policies.UseDurableOutboxOnAllSendingEndpoints();

            options.Policies.UseDurableInboxOnAllListeners();

            options
                .UseRedisTransport(
                    (sp) => sp.GetRequiredKeyedService<IConnectionMultiplexer>("messaging")
                )
                .AutoProvision();

            options.Policies.AutoApplyIdempotencyOnNonTransactionalHandlers();
            options.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;
            // Configure transient exceptions to retry with a cooldown
            options
                .Policies.OnException<HttpRequestException>()
                .RetryWithCooldown(
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(30)
                );
        });
    }
}
