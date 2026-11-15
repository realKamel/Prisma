using System.Text.RegularExpressions;
using Prisma.Domain.ValueObjects.UserDomain;

namespace Prisma.Application.Features.Teachers.Profile;

internal enum ContactPlatform
{
    Whatsapp,
    Facebook,
    Instagram,
    Youtube,
    Telegram,
    Tiktok,
    Website,
}

internal readonly record struct LinkResult(string? Value, string? ErrorKey)
{
    public bool IsValid => ErrorKey is null;
}

/// <summary>
/// C# port of the Angular `normalizeLink`. Accepts a username or a link and returns the canonical
/// https URL (WhatsApp: digits only). The server re-checks everything the form checks.
/// </summary>
internal static partial class ContactLinkNormalizer
{
    private const int MaxLinkLength = 200;

    private static readonly LinkResult Invalid = new(null, "VALIDATION.LINK_INVALID");

    private static readonly Dictionary<ContactPlatform, (string[] Hosts, string BaseUrl)> Sites = new()
    {
        [ContactPlatform.Facebook] = (new[] { "facebook.com", "fb.com", "fb.me" }, "https://facebook.com/"),
        [ContactPlatform.Instagram] = (new[] { "instagram.com" }, "https://instagram.com/"),
        [ContactPlatform.Youtube] = (new[] { "youtube.com", "youtu.be" }, "https://youtube.com/"),
        [ContactPlatform.Telegram] = (new[] { "t.me", "telegram.me" }, "https://t.me/"),
        [ContactPlatform.Tiktok] = (new[] { "tiktok.com" }, "https://tiktok.com/"),
    };

    /// <summary>Platforms whose profile URLs are written /@handle.</summary>
    private static readonly HashSet<ContactPlatform> AtHandle =
    [
        ContactPlatform.Youtube,
        ContactPlatform.Tiktok,
    ];

    [GeneratedRegex(@"^01[0125][0-9]{8}$")]
    private static partial Regex EgyptianMobile();

    [GeneratedRegex(@"^@?[A-Za-z0-9._-]{2,64}$")]
    private static partial Regex Handle();

    [GeneratedRegex(@"^(https?://|www\.)", RegexOptions.IgnoreCase)]
    private static partial Regex UrlPrefix();

    [GeneratedRegex(@"^https?://", RegexOptions.IgnoreCase)]
    private static partial Regex HttpPrefix();

    [GeneratedRegex(@"[\s-]")]
    private static partial Regex WhitespaceOrDash();

    /// <summary>Normalises every link; call only after validation passed.</summary>
    public static ContactLinks Normalize(ContactLinksDto dto) =>
        new()
        {
            Whatsapp = Normalize(ContactPlatform.Whatsapp, dto.Whatsapp).Value,
            Facebook = Normalize(ContactPlatform.Facebook, dto.Facebook).Value,
            Instagram = Normalize(ContactPlatform.Instagram, dto.Instagram).Value,
            Youtube = Normalize(ContactPlatform.Youtube, dto.Youtube).Value,
            Telegram = Normalize(ContactPlatform.Telegram, dto.Telegram).Value,
            Tiktok = Normalize(ContactPlatform.Tiktok, dto.Tiktok).Value,
            Website = Normalize(ContactPlatform.Website, dto.Website).Value,
        };

    public static LinkResult Normalize(ContactPlatform platform, string? raw)
    {
        var text = (raw ?? string.Empty).Trim();
        if (text.Length == 0)
        {
            return new LinkResult(null, null);
        }

        if (platform == ContactPlatform.Whatsapp)
        {
            var digits = WhitespaceOrDash().Replace(text, string.Empty);
            return EgyptianMobile().IsMatch(digits)
                ? new LinkResult(digits, null)
                : new LinkResult(null, "VALIDATION.EGYPTIAN_MOBILE");
        }

        if (text.Length > MaxLinkLength)
        {
            return Invalid;
        }

        var looksLikeUrl =
            platform == ContactPlatform.Website || UrlPrefix().IsMatch(text) || text.Contains('/');

        if (!looksLikeUrl)
        {
            if (!Handle().IsMatch(text))
            {
                return Invalid;
            }

            var baseUrl = Sites[platform].BaseUrl;
            var prefix = AtHandle.Contains(platform) ? "@" : string.Empty;
            return new LinkResult($"{baseUrl}{prefix}{text.TrimStart('@')}", null);
        }

        var candidate = HttpPrefix().IsMatch(text) ? text : $"https://{text}";
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            return Invalid;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return Invalid;
        }

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            return Invalid;
        }

        if (uri.Scheme == Uri.UriSchemeHttp)
        {
            uri = new UriBuilder(uri) { Scheme = Uri.UriSchemeHttps, Port = -1 }.Uri;
        }

        var host = uri.Host.ToLowerInvariant();
        if (host.StartsWith("www.", StringComparison.Ordinal))
        {
            host = host[4..];
        }

        if (platform == ContactPlatform.Website)
        {
            return host.Contains('.') ? new LinkResult(uri.AbsoluteUri, null) : Invalid;
        }

        var matches = Sites[platform]
            .Hosts.Any(h => host == h || host.EndsWith($".{h}", StringComparison.Ordinal));

        return matches
            ? new LinkResult(uri.AbsoluteUri, null)
            : new LinkResult(null, "VALIDATION.LINK_WRONG_SITE");
    }
}
