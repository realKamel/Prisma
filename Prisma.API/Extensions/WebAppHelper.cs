using System.Globalization;
using System.Text;
using Ardalis.Result.AspNetCore;
using Hangfire;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Prisma.API.Filters;
using Prisma.API.Middlewares;
using Prisma.Application;
using Prisma.Application.Abstractions.BackgroundJobs;
using Prisma.Application.Common.Constants;
using Prisma.Infrastructure;
using Prisma.Infrastructure.BackgroundJobs.Jobs;
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

            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = ctx =>
                {
                    ctx.ProblemDetails.Extensions.TryAdd("traceId", ctx.HttpContext.TraceIdentifier);
                };
            });

            services.AddControllers(options =>
                options.AddDefaultResultConvention());

            services.AddOpenApi();

            services.AddExceptionHandler<GlobalExceptionHandler>();

            //Application Services
            services.AddApplicationServices(configuration);

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

            services.AddStackExchangeRedisOutputCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Valkey");
                options.InstanceName = "Prisma_OutputCache_";
            });

            // Add forwarded headers BEFORE anything else that reads the request scheme
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                // Clear known networks and proxies so .NET trusts (Caddy|Traefik)'s headers inside Docker
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });

            //services.AddHealthChecksUI(setup =>
            // {
            //     setup.SetEvaluationTimeInSeconds(30); // How often the UI polls the /health/ready endpoint
            //     setup.AddHealthCheckEndpoint("API Health", "/health/ready");
            // }).AddInMemoryStorage();

            // services.AddOpenAIResponses();
            // services.AddOpenAIConversations();
            // services.AddDevUI();
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
                            context.Token = context.Request.Cookies[AppCookies.AccessToken];
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
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true) // Dev
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            // services.AddAuthorization(options =>
            // {
            //     foreach (var (policy, permissions) in AppClaims.Policies.PermissionMap)
            //     {
            //         options.AddPolicy(policy, builder =>
            //             builder.RequireAssertion(ctx =>
            //                 permissions.All(p =>
            //                     ctx.User.Claims.Any(c => c.Type == AppClaims.PermissionsClaim && c.Value == p))));
            //     }
            // });

            services.AddAuthorization(options =>
            {
                foreach (string policy in AppClaims.Policies.All)
                {
                    options.AddPolicy(policy, p =>
                        p.RequireClaim(AppClaims.PermissionsClaim, policy));
                }
            });
        }
    }

    extension(WebApplication app)
    {
        public async Task UseDataSeedingAsync()
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
            await services.SeedAppDataAsync();
        }

        public void UseRecurringJobs()
        {
            using IServiceScope scope = app.Services.CreateScope();
            IBackgroundJobService jobService = scope.ServiceProvider.GetRequiredService<IBackgroundJobService>();

            //Every Friday at 10:00 PM
            jobService.AddOrUpdateRecurring<ReportGenerationJob>(
                JobQueues.Reports,
                x => x.GenerateWeekly(),
                Cron.Weekly(DayOfWeek.Friday, 22, 0));
        }

        public void UseHangfireUi()
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = [new HangfireDashboardAuthFilter()] //TODO: restrict to admins
            });
        }

        public void MapAppHealthChecks()
        {
            // 1. Liveness Probe (Lightweight)
            // Kubernetes uses this to know if the app process is alive. 
            // We exclude heavy checks (like DB) so a temporary DB blip doesn't restart the whole pod.
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = check => !check.Tags.Contains("ready"), // Runs checks WITHOUT the "ready" tag
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            // 2. Readiness Probe (Heavyweight)
            // Kubernetes / Load Balancers use this to know if the app can accept traffic.
            // This includes Postgres, Valkey, and Hangfire (as we tagged them with "ready").
            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        }

        public void MapOpenAiResponses(IHostEnvironment environment)
        {
            app.MapOpenAIResponses();
            app.MapOpenAIConversations();
            if (environment.IsDevelopment())
            {
                // Map DevUI endpoint to /devui
                // app.MapDevUI();
            }
        }

        public void UseLocalization()
        {
            var supportedCultures = new[] { new CultureInfo("en-US"), new CultureInfo("ar-EG") };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures, // For dates, numbers, currency
                SupportedUICultures = supportedCultures // For string localizations
            });
        }
    }
}