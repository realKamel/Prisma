namespace Prisma.Application.Abstractions.Encryption;

public interface IWebhookSignatureVerifier
{
    bool Verify(
        ReadOnlyMemory<byte> rawBody,
        string signature,
        string version,
        string algorithm,
        string encryptedReadOnlyApiKey
    );
}
