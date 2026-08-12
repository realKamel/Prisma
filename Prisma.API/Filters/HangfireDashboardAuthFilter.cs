using Hangfire.Dashboard;
using Prisma.Application.Common.Constants;

namespace Prisma.API.Filters;

public class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        var env = httpContext.RequestServices.GetRequiredService<IHostEnvironment>();

        // Allow unrestricted access in local Development environment
        if (env.IsDevelopment())
        {
            return true;
        }

        // Production 
        return httpContext.User.Identity?.IsAuthenticated == true
               && httpContext.User.IsInRole(AppRoles.Admin);
    }
}