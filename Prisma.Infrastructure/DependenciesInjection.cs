using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prisma.Application.Abstractions.Identity;
using Prisma.Infrastructure.Services.Identity;
using Prisma.Domain.Entities;
using Prisma.Domain.Interfaces;
using Prisma.Infrastructure.Persistence;
using Prisma.Infrastructure.Persistence.Interceptors;
using Prisma.Infrastructure.Persistence.Repositories;

namespace Prisma.Infrastructure;

public static class DependenciesInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddHttpContextAccessor();
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultSqlConnection"), npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            });

            options.AddInterceptors(serviceProvider
                .GetRequiredService<AuditInterceptor>());

            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });
        services.AddIdentity<User, IdentityRole<Guid>>()
            .AddRoleManager<RoleManager<IdentityRole<Guid>>>()
            .AddSignInManager<SignInManager<User>>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        //TODO:Implement Current User Services
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
    }
}