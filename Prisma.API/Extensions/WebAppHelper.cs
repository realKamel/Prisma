using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Prisma.API.Middlewares;
using Prisma.Application;
using Prisma.Application.Common.Constants;
using Prisma.Infrastructure;
using Prisma.Infrastructure.Authorization;
using Prisma.Infrastructure.Services.Auth;
using Prisma.Infrastructure.Services.DataSeeding;
using Serilog;

namespace Prisma.API.Extensions;

public static class WebAppHelper
{
    extension(IServiceCollection services)
    {
        public void AddWebAppServices(IConfiguration configuration, IHostEnvironment hostEnvironment)
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

            services.AddOutputCache(options =>
            {
                //  Default policy for ALL endpoints
                options.AddBasePolicy(builder =>
                    builder.Expire(TimeSpan.FromSeconds(10)));

                // Named policies
                options.AddPolicy(CachePolicyNames.Short.Name, builder =>
                    builder.Expire(CachePolicyNames.Short.Duration));

                options.AddPolicy(CachePolicyNames.Long.Name, builder =>
                    builder.Expire(CachePolicyNames.Long.Duration));
            });

            // Add forwarded headers BEFORE anything else that reads the request scheme
            //services.Configure<ForwardedHeadersOptions>(options =>
            //{
            //    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            //    // Trust Caddy (Docker internal network) — safe since Caddy is only entry point
            //    options.KnownIPNetworks.Clear();
            //    options.KnownProxies.Clear();
            //});
        }

        private void AddJwtAuthentication(IConfiguration configuration,
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
                            context.Token = context.Request.Cookies[AppClaims.Cookies.AccessToken];
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
                            ClockSkew = TimeSpan.Zero
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

            services.AddCors(options =>
            {
                options.AddPolicy("Dev", policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:4200", // Dev Angular
                            "https://localhost:4200") // If Angular also behind proxy
                                                      //"https://PrismaEdu.com"           // Prod
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        }
    }

    public static async Task UseDataSeedingAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
            await services.SeedAppDataAsync();
        }
    }
}