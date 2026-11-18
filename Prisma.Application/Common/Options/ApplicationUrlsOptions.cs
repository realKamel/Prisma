using System.ComponentModel.DataAnnotations;

namespace Prisma.Application.Common.Options;

public sealed class ApplicationUrlsOptions
{
    public const string SectionName = "ApplicationUrls";

    [Required, Url]
    public required string BackendUrl { get; set; }

    [Required, Url]
    public required string FrontendUrl { get; set; }

    /// <summary>
    /// Composes an absolute backend URL from path fragments.
    /// Fragments are concatenated with '/'; each is trimmed of leading/trailing slashes.
    /// Fragments must be trusted path data — '?', '#' and '%' are not escaped.
    /// </summary>
    public Uri Backend(params string[] fragments) => Combine(BackendUrl, Join(fragments));

    /// <summary>Composes an absolute frontend URL from path fragments. See <see cref="Backend(string[])"/>.</summary>
    public Uri Frontend(params string[] fragments) => Combine(FrontendUrl, Join(fragments));

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

    private static string Join(string[] fragments)
    {
        ArgumentNullException.ThrowIfNull(fragments);
        if (fragments.Length == 0)
        {
            throw new ArgumentException("At least one fragment is required.", nameof(fragments));
        }

        var parts = new string[fragments.Length];
        for (var i = 0; i < fragments.Length; i++)
        {
            var fragment =
                fragments[i]
                ?? throw new ArgumentException(
                    $"Fragment at index {i} is null.",
                    nameof(fragments)
                );

            parts[i] = fragment.Trim('/');
        }

        // drop empties so "a", "", "b" doesn't become "a//b"
        return string.Join('/', parts.Where(p => p.Length != 0));
    }
}
