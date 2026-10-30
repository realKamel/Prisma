namespace Prisma.Infrastructure.ExternalServices.EmailService;

public sealed class ResendOptions
{
    public const string SectionName = "ResendOptions";
    public string ApiKey { get; set; }
    public string From { get; set; }
}
