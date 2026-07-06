using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Infrastructure.Services.StorageService;

public class MuxTokenService(IConfiguration configuration) : IMuxTokenService
{
    public string GeneratePlaybackToken(string playbackId, int expiryHours = 6)
    {
        var keyId = configuration["Mux:SigningKeyId"]!;
        var privateKeyBase64 = configuration["Mux:SigningPrivateKey"]!;

        var privateKeyBytes = Convert.FromBase64String(privateKeyBase64);
        var pemString = Encoding.UTF8.GetString(privateKeyBytes);

        var rsa = RSA.Create();
        rsa.ImportFromPem(pemString);

        var privateKey = new RsaSecurityKey(rsa);

        var signingCredentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256)
        {
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
        };

        var token = new JwtSecurityToken(
            claims: new[]
            {
                new Claim("sub", playbackId),
                new Claim("aud", "v"),
                new Claim("kid", keyId),
            },
            expires: DateTime.UtcNow.AddHours(expiryHours),
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}