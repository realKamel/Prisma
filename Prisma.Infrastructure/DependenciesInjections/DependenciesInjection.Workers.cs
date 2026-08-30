using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Abstractions.BackgroundJobs;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Infrastructure.BackgroundJobs;
using Prisma.Infrastructure.BackgroundJobs.Jobs;
using Prisma.Infrastructure.Services.StorageService;

namespace Prisma.Infrastructure.DependenciesInjections;

public static partial class DependenciesInjection
{
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
    }
}
