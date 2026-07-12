using Microsoft.AspNetCore.Authorization;

namespace Prisma.Infrastructure.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public class HasPermissionAttribute : AuthorizeAttribute
{
    /// <param name="permission">The permission string constant from AppClaims.Permissions</param>
    public HasPermissionAttribute(string permission) : base(policy: permission)
    {
    }
}