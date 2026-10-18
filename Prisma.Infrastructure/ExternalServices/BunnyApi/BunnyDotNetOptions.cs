namespace Prisma.Infrastructure.ExternalServices.BunnyApi;

public class BunnyDotNetOptions
{
    public const string SectionName = "BunnyDotNet";

    // "CoreBaseUrl": "https://api.bunny.net",
    public string CoreBaseUrl { get; set; }

    // "StreamBaseUrl": "https://video.bunnycdn.com",
    public string StreamBaseUrl { get; set; }

    // "ApiKey": "",
    public string ApiKey { get; set; }

    // "DefaultPullZone": ""
    public string DefaultPullZone { get; set; }
}
