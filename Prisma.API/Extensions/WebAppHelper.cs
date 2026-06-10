using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Prisma.API.Middlewares;
using Prisma.Application;
using Prisma.Application.Common.Constants;
using Prisma.Infrastructure;
using Prisma.Infrastructure.Services.Auth;
using Prisma.Infrastructure.Services.Identity;
using Serilog;

namespace Prisma.API.Extensions;

public static class WebAppHelper
{
    public static void AddWebAppServices(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        // web api services
        services.AddSerilog((sp, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext());
        services.AddControllers();
        services.AddOpenApi();

        services.AddScoped<GlobalExceptionHandlingMiddleware>();

        //Application Services
        services.AddApplicationServices();

        //Infrastructure Services
        services.AddInfrastructureServices(configuration, hostEnvironment);

        services.AddJwtAuthentication(configuration, hostEnvironment);
    }

    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
        var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["access_token"];
                        return Task.CompletedTask;
                    }
                };

                if (hostEnvironment.IsDevelopment())
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        // ValidIssuer = jwtSettings.Issuer,
                        // ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        // ClockSkew = TimeSpan.Zero
                    };
                }
                else
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ClockSkew = TimeSpan.Zero
                    };
                }

                options.RequireHttpsMetadata = !hostEnvironment.IsDevelopment();
            });

        //introduce more policies when needed
        services.AddAuthorizationBuilder()
            .AddPolicy(AppClaims.Policies.CanManageCourses, policy =>
                policy.RequireClaim(AppClaims.PermissionsClaim,
                    AppClaims.Permissions.ManageCourses));
    }

    public static async Task UseDataSeedingAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider.GetRequiredService<IIdentitySeeder>();
                await services.SeedRolesAsync();
                await services.SeedUsersAsync();
            }
        }
    }
}