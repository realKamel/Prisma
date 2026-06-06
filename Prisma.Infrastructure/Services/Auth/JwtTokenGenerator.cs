using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Prisma.Application.Abstractions.Auth;
using Prisma.Application.Common.Constants;
using Prisma.Infrastructure.Services.Auth;

namespace Prisma.Infrastructure.Services.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly SigningCredentials _signingCredentials;
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(IOptions<JwtSettings> jwtSettings)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Secret));
        _jwtSettings = jwtSettings.Value;
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public string GenerateAccessToken(Guid userId, string email, IList<string> permissions)
    {
        var userClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        userClaims.AddRange(permissions.Select(p =>
            new Claim(AppClaims.PermissionsClaim, p)));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: userClaims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            signingCredentials: _signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var range = RandomNumberGenerator.Create();
        range.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}