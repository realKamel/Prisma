using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prisma.Application.Abstractions.Services.ObjectStorage;
using Prisma.Infrastructure.Services.ObjectStorageServices;
using Prisma.Infrastructure.Services.StorageService;

namespace Prisma.Infrastructure.DependenciesInjections;

public static partial class DependenciesInjection
{
    private static void AddObjectStorageServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment
    )
    {
        var storageConfig = configuration
            .GetSection(ObjectStorageOptions.SectionName)
            .Get<ObjectStorageOptions>();

        ArgumentNullException.ThrowIfNull(storageConfig);

        services.AddSingleton<IAmazonS3>(_ =>
        {
            if (hostEnvironment.IsDevelopment())
            {
                var config = new AmazonS3Config
                {
                    ServiceURL = storageConfig.ServiceUrl,
                    ForcePathStyle = storageConfig.ForcePathStyle,
                    UseHttp = true,
                };
                return new AmazonS3Client(storageConfig.AccessKey, storageConfig.SecretKey, config);
            }

            // Retrieve S3 API credentials for your R2 bucket via API tokens (see: https://developers.cloudflare.com/r2/api/tokens)
            var accessKey = storageConfig.AccessKey;
            var secretKey = storageConfig.SecretKey;
            var credentials = new BasicAWSCredentials(accessKey, secretKey);

            return new AmazonS3Client(
                credentials,
                new AmazonS3Config { ServiceURL = storageConfig.ServiceUrl }
            );
        });

        services
            .AddOptions<ObjectStorageOptions>()
            .Bind(configuration.GetSection(ObjectStorageOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        if (hostEnvironment.IsDevelopment())
        {
            services.AddScoped<IStorageService, S3StorageService>();
        }
        else
        {
            services.AddScoped<IStorageService, R2StorageService>();
        }
    }
}
