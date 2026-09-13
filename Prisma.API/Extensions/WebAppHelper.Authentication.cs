using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Prisma.Application.Common.Constants;
using Prisma.Infrastructure.Services.Auth;

namespace Prisma.API.Extensions;

public static partial class WebAppHelper
{
    private static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration,
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