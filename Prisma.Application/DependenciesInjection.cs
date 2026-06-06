using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Behaviours;

namespace Prisma.Application;

public static class DependenciesInjection
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>();
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));

        // Register all validators from the assembly
        services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

        // Register the pipeline behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    }
}