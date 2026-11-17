using System.ComponentModel.DataAnnotations;

namespace Prisma.Application.Common.Options;

public sealed class ApplicationUrlsOptions
{
    public const string SectionName = "ApplicationUrls";

    [Required, Url]
    public required string BackendUrl { get; set; }

    [Required, Url]
    public required string FrontendUrl { get; set; }

    /// <summary>Composes an absolute backend URL from a relative path.</summary>
    public Uri Backend(string path) => Combine(BackendUrl, path);

    /// <summary>Composes an absolute frontend URL from a relative path.</summary>
    public Uri Frontend(string path) => Combine(FrontendUrl, path);

    private static Uri Combine(string baseUrl, string path)
    {
        // guard the inputs explicitly. Without these, a null `path` produces a
        // NullReferenceException deep inside TrimStart, which is hard to trace back to the caller.
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl);
        ArgumentNullException.ThrowIfNull(path);

        // the base MUST end with '/'.
        // new Uri("https://api.com/v1",  "users") → https://api.com/users    ← /v1 silently dropped
        // new Uri("https://api.com/v1/", "users") → https://api.com/v1/users ← correct
        // Uri treats the last path segment as a file and replaces it with the relative part.
        var baseUri = new Uri(baseUrl.TrimEnd('/') + "/", UriKind.Absolute);

        // the relative part MUST NOT start with '/'.
        // new Uri(base, "/users") → https://api.com/users  ← resets to the domain root
        // TrimStart('/') also neutralizes "//evil.com/x" (protocol-relative form).
        var result = new Uri(baseUri, path.TrimStart('/'));

        // a fully-qualified relative (e.g. "https://evil.com/x") makes Uri discard the
        // base and return the absolute URL verbatim — a silent SSRF / host-confusion bug.
        // Reject anything that escaped the configured host.
        if (result.Scheme != baseUri.Scheme || result.Authority != baseUri.Authority)
        {
            throw new ArgumentException($"Path '{path}' escapes the base URL.", nameof(path));
        }

        // return the Uri directly (not .ToString()).
        // - HttpClient.GetAsync/PostAsync accept Uri natively — no reparsing needed.
        // - If you must return string, use .AbsoluteUri, NOT .ToString():
        // ToString() may un-escape characters for display and produce an invalid wire URL.
        return result;
    }
}
