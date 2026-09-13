using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Prisma.API.Extensions;

public static partial class WebAppHelper
{
    private static void AddObservabilityServices(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithMetrics(m =>
                m.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddPrometheusExporter())
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation() // Tracks incoming HTTP requests
                .AddHttpClientInstrumentation() // Tracks outbound calls (Paymob, Groq, etc.)
                .AddEntityFrameworkCoreInstrumentation() // Tracks DB queries (if using EF Core)
                .AddOtlpExporter(options =>
                {
                    // 'tempo' is the Docker Compose service name. 4317 is the gRPC port.
                    options.Endpoint = new Uri("http://tempo:4317");
                }));
    }
}