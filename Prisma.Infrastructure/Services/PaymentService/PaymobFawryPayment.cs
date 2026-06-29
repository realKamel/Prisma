using Microsoft.Extensions.Options;
using Prisma.Domain.Interfaces;

namespace Prisma.Infrastructure.Services.PaymentService;

public class PaymobFawryService : PaymobService, IPaymentService
{
    public PaymobFawryService(HttpClient http, IOptions<PaymobSettings> options)
        : base(http, options) { }

    public async Task<(string ClientSecret, string PublicKey, string PaymobOrderId)> GetPaymentKeyAsync(
        int amountCents, string email, string firstName, string lastName)
    {
        return await CreateIntentionAsync(amountCents, email, firstName, lastName, Settings.FawryIntegrationId);
    }
}