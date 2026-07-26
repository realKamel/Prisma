using Prisma.API.Extensions;
using Prisma.API.Middlewares;
using Prisma.Infrastructure;
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
                    .WithRootName("Exception"))
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting API...");

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddWebAppServices(builder.Configuration, builder.Environment);
            builder.AddAiAgents(builder.Configuration);
            // builder.AddWorkflows();

            var app = builder.Build();

            await app.UseDataSeedingAsync();

            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

            // Enable forwarded headers if running behind IIS/Reverse Proxy (recommended for MonsterASP)
            // app.UseForwardedHeaders();

            app.UseHttpsRedirection();
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI((options) =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "Prisma API V1");
                    options.RoutePrefix = "swagger";
                });
            }

            app.UseRouting();

            // 1. CORS Policy
            app.UseCors("CorsPolicy");

            // 2. Static Files (Serves index.html, JS, CSS from wwwroot)
            app.UseStaticFiles();

            app.UseHangfireUi();
            
            app.UseAuthentication();
            
            app.UseOutputCache();
            
            app.UseAuthorization();
            
            app.UseRecurringJobs();

            app.MapHealthChecks();
            
            app.UseLocalization();

            // app.MapOpenAiResponses(app.Environment);

            // 3. API Controllers
            app.MapControllers();

            // 4. Angular SPA Fallback Route (Must be last)
            app.MapFallbackToFile("index.html");

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