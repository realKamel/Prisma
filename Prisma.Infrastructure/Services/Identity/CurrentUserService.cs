using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Prisma.Application.Abstractions.Identity;

namespace Prisma.Infrastructure.Services.Identity;

public class CurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; private set; }

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        if (Guid.TryParse(httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier),
                out Guid id))
        {
            UserId = id;
        }
    }
}