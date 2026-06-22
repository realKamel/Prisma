using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Prisma.Application.Common.Constants;

namespace Prisma.Infrastructure.Authorization;

public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if the policy already exists in standard registration
        var policy = await base.GetPolicyAsync(policyName);
        if (policy != null)
        {
            return policy;
        }

        // If the policy doesn't exist, create it dynamically using the permission string
        return new AuthorizationPolicyBuilder()
            .RequireClaim(AppClaims.PermissionsClaim, policyName)
            .Build();
    }
}