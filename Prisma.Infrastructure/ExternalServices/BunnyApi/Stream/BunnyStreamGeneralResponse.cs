using System.Text.Json.Serialization;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Stream;

public record BunnyStreamGeneralResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; init; }
}
