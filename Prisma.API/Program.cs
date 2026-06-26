using Prisma.API.Extensions;
using Prisma.API.Middlewares;
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

            var app = builder.Build();

            await app.UseDataSeedingAsync();

            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

            //app.UseForwardedHeaders();
            //app.UseHttpsRedirection();
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

            // ═══════════════════════════════════════════════════
            // CORRECT MIDDLEWARE ORDER
            // ═══════════════════════════════════════════════════
            app.UseRouting();              // 1. Routing first
            
            app.UseCors("CorsPolicy");     // 2. CORS before Auth
            
            app.UseAuthentication();       // 3. Auth
            
            app.UseOutputCache();          // 4. Cache
            
            app.UseAuthorization();        // 5. AuthZ
            
            app.MapControllers();          // 6. Endpoints last

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