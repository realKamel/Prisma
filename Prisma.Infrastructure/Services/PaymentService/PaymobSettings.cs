namespace Prisma.Infrastructure.Services.PaymentService;

public class PaymobSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string IframeId { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string HmacSecret { get; set; } = string.Empty;
    public int CardIntegrationId { get; set; }
    public int FawryIntegrationId { get; set; }
}