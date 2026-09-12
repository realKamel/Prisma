using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Ardalis.Result.AspNetCore;
using Asp.Versioning;
using Hangfire;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Prisma.API.Common.RateLimitConfigurations;
using Prisma.API.Filters;
using Prisma.API.Localization;
using Prisma.API.Middlewares;
using Prisma.Application;
using Prisma.Application.Abstractions.BackgroundJobs;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Infrastructure.BackgroundJobs.Jobs;
using Prisma.Infrastructure.DependenciesInjections;
using Prisma.Infrastructure.Services.Auth;
using Prisma.Infrastructure.Services.DataSeeding;
using RedisRateLimiting;
using Serilog;
using StackExchange.Redis;

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

            services.AddApiVersioning(options =>
                {
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ReportApiVersions = true; // adds api-supported-versions / api-deprecated-versions headers
                    options.ApiVersionReader = new UrlSegmentApiVersionReader(); // e.g. /api/v1/users
                })
                .AddMvc() // or omit for Minimal APIs
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                }).AddOpenApi();

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

            services.AddLocalizationServices();
            services.AddRateLimiterConfiguration(configuration);

            services.AddObservabilityServices();
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

        private void AddLocalizationServices()
        {
            services.AddLocalization();
            services.AddTransient<IAppLocalizer, AppLocalizer>();
        }

        private void AddRateLimiterConfiguration(IConfiguration configuration)
        {
            var settings = configuration.GetSection(RateLimitSettings.SectionName).Get<RateLimitSettings>()
                           ?? throw new InvalidOperationException("RateLimiting configuration is missing.");

            services.AddRateLimiter((options) =>
            {
                //clients get 429
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Policy 1: AUTH endpoints (login, register, refresh)
                // Strict: 10 attempts / min / IP, No brute force & credential stuffing.
                // Sliding window = precise rolling enforcement, no window-edge bursts.
                options.AddPolicy(RateLimitPolicies.Auth, httpContext =>
                {
                    var multiplexer = httpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();

                    return RedisRateLimitPartition.GetSlidingWindowRateLimiter(
                        partitionKey: $"rl:auth:ip:{httpContext.Connection.RemoteIpAddress}",
                        factory: _ => new RedisSlidingWindowRateLimiterOptions
                        {
                            ConnectionMultiplexerFactory = () => multiplexer,
                            PermitLimit = settings.Auth.MaxRequestsPerMinute,
                            Window = TimeSpan.FromMinutes(1)
                        });
                });

                //Policy 2: PUBLIC endpoints (catalogs, etc.)
                // Token bucket per IP: 100 req/min with burst tolerance.
                options.AddPolicy(RateLimitPolicies.Public, httpContext =>
                {
                    var multiplexer = httpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();

                    return RedisRateLimitPartition.GetTokenBucketRateLimiter(
                        partitionKey: $"rl:public:ip:{httpContext.Connection.RemoteIpAddress}",
                        factory: _ => new RedisTokenBucketRateLimiterOptions
                        {
                            ConnectionMultiplexerFactory = () => multiplexer,
                            TokenLimit = settings.Public.BurstLimit, // Max instant capacity
                            TokensPerPeriod = settings.Public.SustainedPerMinute, // Refill rate
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1)
                        });
                });

                // Policy 3: PROTECTED endpoints (authenticated users)
                // Per USER ID (from JWT), not per IP — logged-in users get their own bucket.
                options.AddPolicy(RateLimitPolicies.UserRead, httpContext =>
                {
                    var multiplexer = httpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                    return RedisRateLimitPartition.GetTokenBucketRateLimiter(
                        partitionKey: $"rl:user:read:{httpContext.User.FindFirst("sub")?.Value ?? "anon"}",
                        factory: _ => new RedisTokenBucketRateLimiterOptions
                        {
                            ConnectionMultiplexerFactory = () => multiplexer,
                            TokenLimit = settings.User.ReadBurstLimit,
                            TokensPerPeriod = settings.User.ReadSustainedPerMinute,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1)
                        });
                });


                // 4. USER WRITE: Token Bucket (Strict for costly operations like DB inserts/LLM)
                options.AddPolicy(RateLimitPolicies.UserWrite, httpContext =>
                {
                    var redis = httpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();

                    return RedisRateLimitPartition.GetTokenBucketRateLimiter(
                        partitionKey: $"rl:user:write:{httpContext.User.FindFirst("sub")?.Value ?? "anon"}",
                        factory: _ => new RedisTokenBucketRateLimiterOptions
                        {
                            ConnectionMultiplexerFactory = () => redis,
                            TokenLimit = settings.User.WriteBurstLimit,
                            TokensPerPeriod = settings.User.WriteSustainedPerMinute,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1)
                        });
                });
                // Policy 5: LLM STREAMING endpoints
                // Concurrency limit: Max 2 simultaneous streaming requests per user.
                // QueueLimit = 0 ensures we fail fast (429) instead of making the user wait in a queue 
                // while their other streams are still running.
                options.AddPolicy(RateLimitPolicies.LlmConcurrency, httpContext =>
                {
                    var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? httpContext.User.FindFirstValue("sub")
                                 ?? "anon";

                    var multiplexer = httpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();

                    return RedisRateLimitPartition.GetConcurrencyRateLimiter(
                        partitionKey: $"rl:llm:concurrent:{userId}",
                        factory: _ => new RedisConcurrencyRateLimiterOptions
                        {
                            ConnectionMultiplexerFactory = () => multiplexer,
                            PermitLimit = settings.Llm.MaxConcurrentStreams,
                            QueueLimit = 0 // CRITICAL: Reject immediately, do not queue LLM requests
                        });
                });

                // Shared 429 response with Retry-After + problem+json
                options.OnRejected = async (context, ct) =>
                {
                    var response = context.HttpContext.Response;

                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                    }

                    response.StatusCode = 429;
                    response.ContentType = "application/problem+json";

                    await response.WriteAsJsonAsync(
                        new
                        {
                            type = "https://datatracker.ietf.org/doc/html/rfc6585#section-4",
                            title = "Too Many Requests",
                            status = 429,
                            detail =
                                "Rate limit exceeded. Please slow down and retry after the duration specified in the Retry-After header.",
                            traceId = context.HttpContext.TraceIdentifier
                        }, cancellationToken: ct);
                };
            });
        }

        private void AddObservabilityServices()
        {
            services.AddOpenTelemetry()
                .WithMetrics(m =>
                    m.AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddMeter("Microsoft.AspNetCore.Hosting")
                        .AddPrometheusExporter())
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation() // Tracks incoming HTTP requests
                    .AddHttpClientInstrumentation() // Tracks outbound calls (Paymob, Groq, etc.)
                    .AddEntityFrameworkCoreInstrumentation() // Tracks DB queries (if using EF Core)
                    .AddOtlpExporter(options =>
                    {
                        // 'tempo' is the Docker Compose service name. 4317 is the gRPC port.
                        options.Endpoint = new Uri("http://tempo:4317");
                    }));
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
            // We exclude heavy checks (like DB), so a temporary DB blip doesn't restart the whole pod.
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = check => !check.Tags.Contains("ready"), // Runs checks WITHOUT the "ready" tag
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            // 2. Readiness Probe (Heavyweight)
            // Kubernetes / Load Balancers use this to know if the app can accept traffic.
            // This includes Postgres, Valkey, and Hangfire (as we tagged them with "ready").
            app.MapHealthChecks("/health/ready",
                new HealthCheckOptions
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