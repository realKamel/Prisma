using Hangfire.Dashboard;
using Prisma.Application.Common.Constants;

namespace Prisma.API.Filters;

public class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        //var httpContext = context.GetHttpContext();

        // Option 1: Only allow authenticated admins
        //return httpContext.User.IsInRole(AppRoles.Admin);

        // Option 2: For development only
        return true;
    }
}