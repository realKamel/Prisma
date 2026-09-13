using System.Globalization;
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
using Prisma.Infrastructure.BackgroundJobs.Jobs;
using Prisma.Infrastructure.DependenciesInjections;
using Prisma.Infrastructure.Services.DataSeeding;
using Serilog;


namespace Prisma.API.Extensions;

public static partial class WebAppHelper
{
    public static void AddWebAppServices(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        // web api services
        services.AddSerilog((sp, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext());

        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                ctx.ProblemDetails.Extensions.TryAdd("traceId", ctx.HttpContext.TraceIdentifier);
            };
        });

        services.AddControllers(options =>
            options.AddDefaultResultConvention());

        services.AddApiVersioningConfiguration();

        services.AddExceptionHandler<GlobalExceptionHandler>();

        //Application Services
        services.AddApplicationServices(configuration);

        //Infrastructure Services
        services.AddInfrastructureServices(configuration, hostEnvironment);

        services.AddJwtAuthentication(configuration, hostEnvironment);
        services.AddOutputCacheServices(configuration);
        services.AddLocalizationServices();
        services.AddRateLimiterConfiguration(configuration);
        services.AddObservabilityServices();

        // Add forwarded headers BEFORE anything else that reads the request scheme
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

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
    }

    private static void AddLocalizationServices(this IServiceCollection services)
    {
        services.AddLocalization();
        services.AddTransient<IAppLocalizer, AppLocalizer>();
    }

    extension(WebApplication app)
    {
        public async Task UseDataSeedingAsync()
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
            await services.SeedAppDataAsync();
        }

        public void UseRecurringJobs()
        {
            using IServiceScope scope = app.Services.CreateScope();
            IBackgroundJobService jobService = scope.ServiceProvider.GetRequiredService<IBackgroundJobService>();

            //Every Friday at 10:00 PM
            jobService.AddOrUpdateRecurring<ReportGenerationJob>(
                JobQueues.Reports,
                x => x.GenerateWeekly(),
                Cron.Weekly(DayOfWeek.Friday, 22, 0));
        }

        public void UseHangfireUi()
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = [new HangfireDashboardAuthFilter()] //TODO: restrict to admins
            });
        }

        public void MapAppHealthChecks()
        {
            // 1. Liveness Probe (Lightweight)
            // Kubernetes uses this to know if the app process is alive. 
            // We exclude heavy checks (like DB), so a temporary DB blip doesn't restart the whole pod.
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = check => !check.Tags.Contains("ready"), // Runs checks WITHOUT the "ready" tag
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            // 2. Readiness Probe (Heavyweight)
            // Kubernetes / Load Balancers use this to know if the app can accept traffic.
            // This includes Postgres, Valkey, and Hangfire (as we tagged them with "ready").
            app.MapHealthChecks("/health/ready",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("ready"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
        }

        public void MapOpenAiResponses(IHostEnvironment environment)
        {
            app.MapOpenAIResponses();
            app.MapOpenAIConversations();
            if (environment.IsDevelopment())
            {
                // Map DevUI endpoint to /devui
                // app.MapDevUI();
            }
        }

        public void UseLocalization()
        {
            var supportedCultures = new[] { new CultureInfo("en-US"), new CultureInfo("ar-EG") };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures, // For dates, numbers, currency
                SupportedUICultures = supportedCultures // For string localizations
            });
        }
    }
}