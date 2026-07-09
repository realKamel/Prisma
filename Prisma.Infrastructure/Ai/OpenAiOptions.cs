namespace Prisma.Infrastructure.Ai;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public required string ApiKey { get; init; }
    public required string FastChatModel { get; init; }
}