using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using Prisma.Application.Abstractions.Encryption;

namespace Prisma.Infrastructure.Encryption;

public sealed class BunnyWebhookSignatureVerifier(ISecretCipher secretCipher)
    : IWebhookSignatureVerifier
{
    private const string SupportedVersion = "v1";
    private const string SupportedAlgorithm = "hmac-sha256";
    private const int Sha256ByteLength = 32;
    private const int Sha256HexLength = 64;
    private const int MaxStackKeyBytes = 256;

    private static bool IsValid(
        ReadOnlyMemory<byte> rawBody,
        string signature,
        string version,
        string algorithm,
        string encryptedReadOnlyApiKey
    )
    {
        if (
            rawBody.IsEmpty
            || string.IsNullOrWhiteSpace(signature)
            || string.IsNullOrWhiteSpace(encryptedReadOnlyApiKey)
        )
        {
            return false;
        }

        if (
            !string.Equals(version, SupportedVersion, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(algorithm, SupportedAlgorithm, StringComparison.OrdinalIgnoreCase)
        )
        {
            return false;
        }

        if (signature.Length != Sha256HexLength)
        {
            return false;
        }

        return true;
    }

    public bool Verify(
        ReadOnlyMemory<byte> rawBody,
        string signature,
        string version,
        string algorithm,
        string encryptedReadOnlyApiKey
    )
    {
        if (!IsValid(rawBody, signature, version, algorithm, encryptedReadOnlyApiKey))
            return false;

        Span<byte> receivedHash = stackalloc byte[Sha256ByteLength];

        var status = Convert.FromHexString(
            signature,
            receivedHash,
            out int charsConsumed,
            out int bytesWritten
        );

        if (
            status != OperationStatus.Done
            || charsConsumed != Sha256HexLength
            || bytesWritten != Sha256ByteLength
        )
        {
            return false;
        }

        var readOnlyApiKey = secretCipher.Decrypt(encryptedReadOnlyApiKey);

        // The body is already raw bytes, we hash it directly.
        // Only the key needs encoding.
        var keyByteCount = Encoding.UTF8.GetByteCount(readOnlyApiKey);

        byte[]? rentedKey = null;

        try
        {
            Span<byte> keyBytes =
                keyByteCount <= MaxStackKeyBytes
                    ? stackalloc byte[MaxStackKeyBytes] // fixed size: safe on stack
                    : (rentedKey = ArrayPool<byte>.Shared.Rent(keyByteCount));

            // Slice down to the actual key length.
            keyBytes = keyBytes[..keyByteCount];

            Encoding.UTF8.GetBytes(readOnlyApiKey, keyBytes);

            Span<byte> expectedHash = stackalloc byte[Sha256ByteLength];

            if (
                !HMACSHA256.TryHashData(
                    keyBytes,
                    rawBody.Span, // no copy: ReadOnlySpan<byte> in
                    expectedHash,
                    out int hashBytesWritten
                )
                || hashBytesWritten != Sha256ByteLength
            )
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(expectedHash, receivedHash);
        }
        finally
        {
            if (rentedKey is not null)
            {
                CryptographicOperations.ZeroMemory(rentedKey.AsSpan(0, keyByteCount));
                ArrayPool<byte>.Shared.Return(rentedKey);
            }
        }
    }
}
