using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace Prisma.Infrastructure.Services.PaymentService;

public abstract class PaymobService
{
    protected readonly HttpClient Http;
    protected readonly PaymobSettings Settings;

    protected PaymobService(HttpClient http, IOptions<PaymobSettings> options)
    {
        Http = http;
        Settings = options.Value;
        Http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Token", Settings.SecretKey);
    }

    protected async Task<(string ClientSecret, string PublicKey, string PaymobOrderId)> CreateIntentionAsync(
        int amountCents, string email, string firstName, string lastName,
        int integrationId)
    {
        var res = await Http.PostAsJsonAsync("https://accept.paymob.com/v1/intention/", new
        {
            amount = amountCents,
            currency = "EGP",
            payment_methods = new[] { integrationId },
            items = new[]
            {
                new { name = "Course", amount = amountCents, quantity = 1 }
            },
            billing_data = new
            {
                first_name = firstName,
                last_name = lastName,
                email,
                phone_number = "NA"
            },
            notification_url = "https://your-backend-link.com/api/v1/payments/webhook",
            redirection_url = "http://your-frontend-link.com/payment/callback"
        });

        var raw = await res.Content.ReadAsStringAsync();
        Console.WriteLine("Paymob intention response: " + raw);

        var data = JsonNode.Parse(raw)?.AsObject();
        var clientSecret = data!["client_secret"]!.ToString();
        var orderId = data!["intention_order_id"]!.ToString();
        return (clientSecret, Settings.PublicKey, orderId);
    }
}