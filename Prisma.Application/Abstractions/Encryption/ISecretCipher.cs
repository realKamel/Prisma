namespace Prisma.Application.Abstractions.Encryption;

public interface ISecretCipher
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
