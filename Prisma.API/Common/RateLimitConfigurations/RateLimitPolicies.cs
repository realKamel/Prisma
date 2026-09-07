namespace Prisma.API.Common.RateLimitConfigurations;

public static class RateLimitPolicies
{
    public const string Auth = "auth";
    public const string Public = "public";
    public const string UserRead = "user_read";
    public const string UserWrite = "user_write";
    public const string LlmConcurrency = "llm_concurrency";
}