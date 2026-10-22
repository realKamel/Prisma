using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Prisma.Infrastructure.RealTime;

/// <summary>
/// Mirrors <c>ICurrentUserService</c> claim resolution so <see cref="Hub.Context" />
/// exposes the same user identifier used by the rest of the application.
/// </summary>
internal sealed class AppUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}