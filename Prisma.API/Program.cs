using Prisma.API.Extensions;
using Prisma.Infrastructure.DependenciesInjections;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;

namespace Prisma.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .Enrich.WithExceptionDetails(
                new DestructuringOptionsBuilder()
                    .WithDefaultDestructurers()
                    .WithRootName("Exception")
            )
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting API...");

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddWebAppServices(builder.Configuration, builder.Environment);
            builder.AddAiAgents(builder.Configuration);
            builder.Host.UseWolverineWithConfiguration(builder.Configuration);
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    "TusLocal",
                    policy =>
                    {
                        policy
                            .WithOrigins("http://localhost:4200")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials()
                            .WithExposedHeaders(
                                "Location",
                                "Tus-Resumable",
                                "Tus-Version",
                                "Tus-Extension",
                                "Tus-Max-Size",
                                "Upload-Offset",
                                "Upload-Length",
                                "Upload-Metadata",
                                "Upload-Defer-Length",
                                "Upload-Concat",
                                "Upload-Expires",
                                "Retry-After"
                            );
                    }
                );
            });

            var app = builder.Build();

            await app.UseDataSeedingAsync();

            // Enable forwarded headers if running behind IIS/Reverse Proxy
            app.UseForwardedHeaders();

            app.UseExceptionHandler();

            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi().WithDocumentPerVersion();
                app.UseDeveloperExceptionPage();
                app.UseSwaggerUI(
                    (options) =>
                    {
                        options.SwaggerEndpoint("/openapi/v1.json", "Prisma API V1");
                        options.RoutePrefix = "swagger";
                    }
                );
            }

            app.UseRouting();

            if (app.Environment.IsDevelopment())
            {
                app.UseCors("TusLocal");
                app.UseCors("CorsPolicy");
            }

            app.UseLocalization();

            app.UseHangfireUi();

            app.UseAuthentication();

            app.UseRateLimiter();

            app.UseOutputCache();

            app.UseAuthorization();

            app.UseRecurringJobs();

            app.MapPrometheusScrapingEndpoint();

            app.MapAppHealthChecks();

            app.MapTusEndpoints();

            app.MapControllers();

            app.MapChatHub();

            await app.RunAsync();
        }
        catch (HostAbortedException)
        {
            throw;
        }
        catch (Exception e)
        {
            Log.Fatal(e, "The API terminated unexpectedly during startup");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
