using Prisma.API.Middlewares;
using Prisma.Application;
using Prisma.Infrastructure;
using Serilog;

namespace Prisma.API.Extensions;

public static class WebAppHelper
{
    public static void AddWebAppServices(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        // web api services
        services.AddSerilog((sp, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext());
        services.AddControllers();
        services.AddOpenApi();

        services.AddScoped<GlobalExceptionHandlingMiddleware>();

        //Application Services
        services.AddApplicationServices();

        //Infrastructure Services
        services.AddInfrastructureServices(configuration, hostEnvironment);
    }
}