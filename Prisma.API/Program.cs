using Prisma.API.Extensions;
using Prisma.API.Middlewares;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;

namespace Prisma.API;

public class Program
{
    public static void Main(string[] args)
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
            
            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
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
            Log.CloseAndFlush();
        }
    }
}