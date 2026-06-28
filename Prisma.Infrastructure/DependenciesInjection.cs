using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Infrastructure.Identity;
using Prisma.Infrastructure.Persistence;
using Prisma.Infrastructure.Persistence.Interceptors;
using Prisma.Infrastructure.Persistence.Repositories;
using Prisma.Infrastructure.Services;
using Prisma.Infrastructure.Services.Auth;
using Prisma.Infrastructure.Services.DataSeeding;
using Prisma.Infrastructure.Services.EmailService;
using Prisma.Infrastructure.Services.PaymentService;
using StackExchange.Redis;

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
                // npgsqlOptions.EnableRetryOnFailure(
                //     maxRetryCount: 5,
                //     maxRetryDelay: TimeSpan.FromSeconds(10),
                //     errorCodesToAdd: null);
            });

            options.AddInterceptors(
            serviceProvider.GetRequiredService<AuditInterceptor>(),
            serviceProvider.GetRequiredService<AuditLogInterceptor>());

            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });
        services.AddIdentityCore<User>(options =>
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

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

        services.AddScoped<AuditInterceptor>();
        services.AddScoped<AuditLogInterceptor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, EmailService>();

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IDataSeeder, DataSeeder>();
        services.AddScoped<IIdentityService, IdentityService>();

        services.AddSingleton<IPdfTextExtractor, PdfTextExtractor>();
        services.AddSingleton<IOpenAiExamExtractor, OpenAiExamExtractor>();
        services.AddSingleton<IExtractionJobQueue, ExtractionJobQueue>();
        services.AddScoped<IFileService, FileService>();

        services.Configure<PaymobSettings>(configuration.GetSection("PaymobSettings"));

        services.AddHttpClient<PaymobCardService>();
        services.AddHttpClient<PaymobFawryService>();

        services.AddKeyedScoped<IPaymentService, PaymobCardService>("card");
        services.AddKeyedScoped<IPaymentService, PaymobFawryService>("fawry");

        //services.AddStackExchangeRedisCache(option =>
        //{
        //    option.Configuration = configuration.GetConnectionString("Redis");
        //});

        //services.AddDataProtection()
        //    .PersistKeysToStackExchangeRedis(
        //        ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")),
        //        "DataProtection-Keys");
    }
}