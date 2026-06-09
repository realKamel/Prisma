using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Prisma.Application.Abstractions.Auth;
using Prisma.Application.Common.Constants;

namespace Prisma.Infrastructure.Services.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly SigningCredentials _signingCredentials;
    private readonly JwtSettings _jwtSettings;
    private readonly SymmetricSecurityKey _securityKey;

    public JwtTokenService(IOptions<JwtSettings> jwtSettings)
    {
        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Secret));
        _jwtSettings = jwtSettings.Value;
        _signingCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);
    }

    public string GenerateAccessToken(Guid userId, string email, IList<string> roles, IList<string>? permissions = null)
    {
        var userClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (permissions is not null && permissions.Count > 0)
        {
            userClaims
                .AddRange(permissions
                    .Select(p =>
                        new Claim(AppClaims.PermissionsClaim, p)));
        }

        userClaims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

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

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = _securityKey
        };
        try

        {
            return new JwtSecurityTokenHandler()
                .ValidateToken(token, validationParams, out _);
        }
        catch { return null; }
    }
}