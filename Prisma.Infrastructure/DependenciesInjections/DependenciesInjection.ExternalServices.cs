using ClamClient.Net.Configuration;
using ClamClient.Net.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Abstractions.Email;
using Prisma.Application.Abstractions.Security;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Abstractions.VideoServices;
using Prisma.Infrastructure.ExternalServices.BunnyApi;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Core;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Stream;
using Prisma.Infrastructure.ExternalServices.EmailService;
using Prisma.Infrastructure.Services.EmailService;
using Prisma.Infrastructure.Services.Security;
using Prisma.Infrastructure.Services.StorageService;
using Prisma.Infrastructure.Services.VideoStreamingAndStorage;
using Refit;
using Resend;

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

        var resendOptions =
            configuration.GetSection(ResendOptions.SectionName).Get<ResendOptions>()
            ?? throw new InvalidOperationException("Resend configuration is missing.");

        ArgumentException.ThrowIfNullOrWhiteSpace(resendOptions.ApiKey);

        services
            .AddOptions<ResendOptions>()
            .Bind(configuration.GetSection(ResendOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<ResendClient>();

        services.Configure<ResendClientOptions>(options =>
        {
            options.ApiToken = resendOptions.ApiKey;
        });

        services.AddTransient<IResend, ResendClient>();

        services.AddTransient<IEmailSender, ResendEmailSender>();
    }

    private static void AddMuxStreaming(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Using IConfiguration (appsettings / environment variables)
        var isContainer =
            bool.TryParse(configuration["DOTNET_RUNNING_IN_CONTAINER"], out var value) && value;
        if (isContainer)
        {
            services.AddScoped<IVideoStorageService, MinioVideoStorageService>();
        }
        else
        {
            services.AddScoped<IVideoStorageService, MuxVideoStorageService>();
        }

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
                // c.DefaultRequestHeaders.Add("Authorization", bunnyOptions.ApiKey);
            })
            .AddStandardResilienceHandler();

        services
            .AddRefitClient<IBunnyCorePullZoneApi>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(bunnyOptions.CoreBaseUrl);
                c.DefaultRequestHeaders.Add("Authorization", bunnyOptions.ApiKey);
            })
            .AddStandardResilienceHandler();

        services.AddScoped<IVideoStreamServices, VideoStreamServices>();
        services.AddScoped<IVideoLibraryService, VideoLibraryService>();
    }

    private static void AddFileScannerService(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var clamAvOptions = configuration
            .GetSection(ClamAvOptions.SectionName)
            .Get<ClamAvOptions>();

        ArgumentNullException.ThrowIfNull(clamAvOptions);
        ArgumentException.ThrowIfNullOrWhiteSpace(clamAvOptions.Host, nameof(clamAvOptions.Host));

        services.Configure<ClamAvOptions>(configuration.GetSection(ClamAvOptions.SectionName));

        services.AddClamClient(options =>
        {
            options.Endpoint = ClamEndpoint.Tcp(clamAvOptions.Host, clamAvOptions.Port);
            options.Timeout = clamAvOptions.Timeout;
            options.MaxStreamSize = clamAvOptions.MaxFileSizeInBytes;
        });

        services.AddScoped<IFileScanner, ClamAvFileScanner>();
    }
}
