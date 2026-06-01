using Serilog;

namespace Prisma.API;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting API...");
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
                            .ReadFrom.Configuration(builder.Configuration)
                            .ReadFrom.Services(services)
                            .Enrich.FromLogContext());

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();

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
