using System.Security.Cryptography;
using System.Text;

namespace Prisma.Application.Features.Lessons.Helpers;

public static class TusUploadSigner
{
    public static string GenerateUploadSignature(
        long libraryId,
        string apiKey,
        long expirationTime,
        Guid videoId
    )
    {
        // 1. Concatenate the values in the exact requested order
        string rawString = $"{libraryId}{apiKey}{expirationTime}{videoId}";

        // 2. Convert the string to a UTF-8 byte array
        byte[] bytes = Encoding.UTF8.GetBytes(rawString);

        // 3. Compute the SHA-256 hash
        byte[] hashBytes = SHA256.HashData(bytes);

        // 4. Convert the byte array to a lowercase hex string
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
