namespace Prisma.API.Common.RateLimitConfigurations;

internal sealed class RateLimitSettings
{
    public const string SectionName = "RateLimitSettings";

    // Auth: Strict, prevents brute-force
    public AuthSettings Auth { get; set; } = new();

    // Public: Anonymous users, moderate limits
    public PublicSettings Public { get; set; } = new();

    // User: Authenticated users, split by cost
    public UserSettings User { get; set; } = new();

    // LLM: Expensive compute, strict concurrency
    public LlmSettings Llm { get; set; } = new();
}

public class AuthSettings
{
    public int MaxRequestsPerMinute { get; set; }
}

public class PublicSettings
{
    public int BurstLimit { get; set; } // Max instant requests (TokenLimit)
    public int SustainedPerMinute { get; set; } // Refill rate (TokensPerPeriod)
}

public class UserSettings
{
    public int ReadBurstLimit { get; set; }
    public int ReadSustainedPerMinute { get; set; }

    public int WriteBurstLimit { get; set; }
    public int WriteSustainedPerMinute { get; set; }
}

public class LlmSettings
{
    public int MaxConcurrentStreams { get; set; }
}