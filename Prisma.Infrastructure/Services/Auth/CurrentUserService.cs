using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Infrastructure.Services.Auth;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            string? value = User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                            User?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out Guid id) ? id : null;
        }
    }

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsInAnyRole(IReadOnlyList<string>? roles, CancellationToken cancellationToken = default)
    {
        if (User is null || roles is null || roles.Count == 0)
        {
            return false;
        }

        return roles.Any(User.IsInRole);
    }
}