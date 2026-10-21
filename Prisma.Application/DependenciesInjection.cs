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

        // Register the pipeline behavior

        // 1. LOGGING (Outermost) - Measures total execution time, including authorization and cache hits.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        // 2. Authorization - Ensures the current user is authorized before accessing cached data or executing the handler
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));

        // 3. CACHING - Returns cached responses when available, skipping validation and handler execution.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        // 4. VALIDATION - Validates requests before executing the handler.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // 5. UNIT OF WORK (Innermost) - Wraps handler execution and commits changes when appropriate.
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
