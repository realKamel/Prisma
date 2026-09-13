using Asp.Versioning;

namespace Prisma.API.Extensions;

public static partial class WebAppHelper
{
    private static void AddApiVersioningConfiguration(this IServiceCollection services)
    {
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
    }
}