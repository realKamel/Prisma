using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Abstractions.Services;
using Prisma.Infrastructure.ExternalServices.BunnyApi;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Core;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Stream;
using Prisma.Infrastructure.Services.EmailService;
using Prisma.Infrastructure.Services.StorageService;
using Refit;

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

    private static void AddBunnyServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var bunnyOptions = configuration
            .GetSection(BunnyDotNetOptions.SectionName)
            .Get<BunnyDotNetOptions>();

        ArgumentNullException.ThrowIfNull(bunnyOptions);
        ArgumentException.ThrowIfNullOrWhiteSpace(bunnyOptions.ApiKey, nameof(bunnyOptions.ApiKey));
        ArgumentException.ThrowIfNullOrWhiteSpace(
            bunnyOptions.ApiKey,
            nameof(bunnyOptions.CoreBaseUrl)
        );
        ArgumentException.ThrowIfNullOrWhiteSpace(
            bunnyOptions.ApiKey,
            nameof(bunnyOptions.StreamBaseUrl)
        );

        services
            .AddRefitClient<IBunnyCoreVideoLibraryApi>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(bunnyOptions.CoreBaseUrl);
                c.DefaultRequestHeaders.Add("Authorization", bunnyOptions.ApiKey);
            })
            .AddStandardResilienceHandler();

        services
            .AddRefitClient<IBunnyStreamVideosApi>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(bunnyOptions.StreamBaseUrl);
                c.DefaultRequestHeaders.Add("Authorization", bunnyOptions.ApiKey);
            })
            .AddStandardResilienceHandler();
    }
}
