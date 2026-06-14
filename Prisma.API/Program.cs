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

            app.UseHttpsRedirection();

            app.UseCors("Dev");

            app.UseRouting();

            app.UseAuthentication();

            app.UseOutputCache();

            app.UseAuthorization();

            app.MapControllers();

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