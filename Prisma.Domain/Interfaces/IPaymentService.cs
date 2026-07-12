namespace Prisma.Domain.Interfaces;

public interface IPaymentService
{
    Task<(string ClientSecret, string PublicKey, string PaymobOrderId)> GetPaymentKeyAsync(
        int amountCents, string email, string firstName, string lastName);
}

