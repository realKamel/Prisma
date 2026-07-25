using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Prisma.Application.Behaviours;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Options;

namespace Prisma.Application;

public static class DependenciesInjection
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>();
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));

        // Register all validators from the assembly
        services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

        // Register the pipeline behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.Configure<FeatureOptions>(configuration.GetSection(AppFeatureKeys.SectionKey));

        services.AddFeatureManagement();

        services.AddOptions<FeatureOptions>()
            .Bind(configuration.GetSection(AppFeatureKeys.SectionKey))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}