namespace Prisma.Application.Common.Interfaces;

public interface ISecretCipher
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
