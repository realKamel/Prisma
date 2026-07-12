using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Infrastructure.Services.Auth;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            string? value = httpContextAccessor
                                .HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                            httpContextAccessor.HttpContext?
                                .User.FindFirstValue(ClaimTypes.NameIdentifier);

            // If UserId is not defined as JwtRegisteredClaimNames.Sub we search for NameIdentifier

            return Guid.TryParse(value, out Guid id) ? id : null;
        }
    }

    public string? Email { get; } = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated { get; } = httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}