using System.Security.Cryptography;
using System.Text;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.Webhooks;

public static class WebhookSignatureValidator
{
    /// <summary>
    /// Validates the signature of an incoming Bunny.net Stream webhook request.
    /// </summary>
    /// <param name="rawBody">The exact raw HTTP request body as a UTF-8 string.</param>
    /// <param name="signatureHeader">The value of the X-BunnyStream-Signature header.</param>
    /// <param name="signatureVersion">The value of the X-BunnyStream-Signature-Version header.</param>
    /// <param name="signatureAlgorithm">The value of the X-BunnyStream-Signature-Algorithm header.</param>
    /// <param name="signatureSecret">Your library's Read-Only API key.</param>
    /// <returns>True if the signature is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static bool Validate(
        string rawBody,
        string signatureHeader,
        string signatureVersion,
        string signatureAlgorithm,
        string signatureSecret
    )
    {
        // 1. Check version
        if (!string.Equals(signatureVersion, "v1", StringComparison.Ordinal))
        {
            return false;
        }

        // 2. Check algorithm
        if (!string.Equals(signatureAlgorithm, "hmac-sha256", StringComparison.Ordinal))
        {
            return false;
        }

        // 3. Validate a signature header format (must be 64 lowercase hex characters)
        if (
            string.IsNullOrEmpty(signatureHeader)
            || signatureHeader.Length != 64
            || !IsLowerCaseHex(signatureHeader)
        )
        {
            return false;
        }

        // 4. Compute the expected signature
        byte[] keyBytes = Encoding.UTF8.GetBytes(signatureSecret);
        byte[] bodyBytes = Encoding.UTF8.GetBytes(rawBody);

        using var hmac = new HMACSHA256(keyBytes);
        byte[] hash = hmac.ComputeHash(bodyBytes);

        // Convert to lowercase hex string
        string expectedSignature = Convert.ToHexString(hash).ToLowerInvariant();

        // 5. Constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedSignature),
            Encoding.UTF8.GetBytes(signatureHeader)
        );
    }

    private static bool IsLowerCaseHex(string input)
    {
        foreach (char c in input)
        {
            if (c is not (>= '0' and <= '9' or >= 'a' and <= 'f'))
            {
                return false;
            }
        }

        return true;
    }
}
