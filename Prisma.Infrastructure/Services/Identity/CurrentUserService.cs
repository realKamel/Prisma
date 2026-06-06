using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Infrastructure.Services.Identity;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var value = httpContextAccessor
                            .HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        httpContextAccessor.HttpContext?
                            .User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            // If UserId is not defined as NameIdentifier we search for JwtRegisteredClaimNames.Sub

            return Guid.TryParse(value, out Guid id) ? id : null;
        }
    }

    public string? Email { get; } = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated { get; } = httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}