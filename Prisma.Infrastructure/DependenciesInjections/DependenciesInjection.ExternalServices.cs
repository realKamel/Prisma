using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Abstractions.Services;
using Prisma.Infrastructure.Services.EmailService;
using Prisma.Infrastructure.Services.StorageService;

namespace Prisma.Infrastructure.DependenciesInjections;

public static partial class DependenciesInjection
{
    private static void AddEmailServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        services.AddScoped<IEmailService, EmailService>();
    }

    private static void AddMuxStreaming(this IServiceCollection services)
    {
        services.AddScoped<IVideoStorageService, MuxVideoStorageService>();
        services.AddScoped<IMuxTokenService, MuxTokenService>();
    }
}
