namespace Prisma.API.Features.Webhooks;

public static class WebhooksHelper
{
    public const int MaxBodyBytes = 64 * 1024;
    public const string JsonMediaType = "application/json";

    public static bool IsJsonContentType(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            return false;
        }

        int semi = contentType.IndexOf(';', StringComparison.InvariantCultureIgnoreCase);
        var mediaType = semi >= 0 ? contentType.AsSpan(0, semi) : contentType.AsSpan();

        return mediaType.Trim().Equals(JsonMediaType, StringComparison.OrdinalIgnoreCase);
    }
}
