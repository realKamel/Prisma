using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Infrastructure.Identity;
using Prisma.Infrastructure.Persistence;
using Prisma.Infrastructure.Services.Auth;

namespace Prisma.Infrastructure.DependenciesInjections;

public static partial class DependenciesInjection
{
    private static void AddIdentityWithConfig(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration
    )
    {
        services.AddHttpContextAccessor();

        services
            .AddIdentityCore<User>(options =>
            {
                //options.User.RequireUniqueEmail = true;
                if (environment.IsDevelopment())
                {
                    options.Password.RequiredLength = 4;
                    options.Password.RequireDigit = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                }
                else
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;

                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                }
            })
            .AddRoles<Domain.Entities.UserAggregate.Role>()
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<AppDbContext>();

        //services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services
            .AddOptions<JwtSettingsOptions>()
            .Bind(configuration.GetSection(JwtSettingsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
    }
}
