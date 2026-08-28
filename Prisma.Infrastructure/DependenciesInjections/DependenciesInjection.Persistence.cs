using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Repositories;
using Prisma.Infrastructure.Persistence;
using Prisma.Infrastructure.Persistence.Interceptors;
using Prisma.Infrastructure.Persistence.Repositories;
using Prisma.Infrastructure.Services.Auth;
using Prisma.Infrastructure.Services.DataSeeding;

namespace Prisma.Infrastructure.DependenciesInjections;

public static partial class DependenciesInjection
{
    private static void AddPersistenceConfig(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        services.AddDbContext<AppDbContext>(
            (serviceProvider, options) =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultSqlConnection"),
                    npgSqlOptions =>
                    {
                        npgSqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorCodesToAdd: null
                        );
                        npgSqlOptions.UseVector();
                    }
                );

                options.AddInterceptors(
                    serviceProvider.GetRequiredService<AuditInterceptor>(),
                    serviceProvider.GetRequiredService<AuditLogInterceptor>()
                );

                if (environment.IsDevelopment())
                {
                    options.EnableDetailedErrors();

                    // ONLY uncomment the line below when you actively need to see SQL parameter values
                    // to debug a specific query. NEVER leave it uncommented in Production.
                    // options.EnableSensitiveDataLogging();
                }
            }
        );

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

        services.AddScoped<AuditInterceptor>();
        services.AddScoped<AuditLogInterceptor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDataSeeder, DataSeeder>();

        //Custom Repositories
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
    }
}
