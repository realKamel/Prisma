using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Prisma.Application;

public static class DependenciesInjection
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<ApplicationRef>();
    }
}