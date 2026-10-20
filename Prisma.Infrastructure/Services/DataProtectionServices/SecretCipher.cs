using Microsoft.AspNetCore.DataProtection;
using Prisma.Application.Common.Interfaces;

namespace Prisma.Infrastructure.Services.DataProtectionServices;

internal sealed class SecretCipher(IDataProtectionProvider provider) : ISecretCipher
{
    private readonly IDataProtector _protector = provider.CreateProtector(
        "SecretProtector.SecretProtector.v1"
    );

    // Purpose strings isolate encryption contexts so keys can't be reused elsewhere

    public string Encrypt(string plainText)
    {
        return string.IsNullOrEmpty(plainText) ? plainText : _protector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {
        return string.IsNullOrEmpty(cipherText) ? cipherText : _protector.Unprotect(cipherText);
    }
}
