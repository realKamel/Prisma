using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Prisma.Application.Common.Behaviors;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Options;

namespace Prisma.Application;

public static class DependenciesInjection
{
    public static void AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>();
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly)
        );

        // Register all validators from the assembly
        services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

        // Register the pipeline behavior

        // 1. LOGGING (Outermost) - Catches total execution time, including cache hits/misses
        //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        // 3. CACHING - Checks FusionCache. If hit, skips Validation and UoW entirely.
        //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(FusionCacheBehavior<,>));

        // 4. VALIDATION - Fails fast before hitting the database.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // 5. UNIT OF WORK (Innermost) - Wraps the actual Handler execution and calls SaveChanges()
        //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DbTransactionBehavior<,>));

        services.Configure<FeatureOptions>(configuration.GetSection(AppFeatureKeys.SectionKey));

        services.AddFeatureManagement();

        services
            .AddOptions<FeatureOptions>()
            .Bind(configuration.GetSection(AppFeatureKeys.SectionKey))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
