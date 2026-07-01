using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Infrastructure.Services.StorageService;

public class StorageBucketPolicyInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public StorageBucketPolicyInitializer(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();

        var bucketName = _configuration["Storage:BucketName"]!;
        var publicPrefixesSection = _configuration.GetSection("Storage:PublicPrefixes").Get<string[]>();

        if (publicPrefixesSection is { Length: > 0 })
        {
            await storageService.SetPublicReadPolicyAsync(bucketName, publicPrefixesSection);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}