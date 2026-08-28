using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Abstractions.Services;
using Prisma.Infrastructure.Services.StorageService;

namespace Prisma.Infrastructure.DependenciesInjections;

public static partial class DependenciesInjection
{
    private static void AddObjectStorageServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var storageConfig = configuration
            .GetSection(ObjectStorageOptions.SectionName)
            .Get<ObjectStorageOptions>();

        ArgumentNullException.ThrowIfNull(storageConfig);

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var config = new AmazonS3Config
            {
                ServiceURL = storageConfig.ServiceUrl,
                ForcePathStyle = storageConfig.ForcePathStyle,
            };

            return new AmazonS3Client(storageConfig.AccessKey, storageConfig.SecretKey, config);
        });

        services.AddScoped<IStorageService, S3StorageService>();
    }
}
