using System.Text.RegularExpressions;
using Ardalis.Result;

namespace Prisma.Domain.ValueObjects.ContentDomain;

public sealed partial record Slug
{
    public string Value { get; init; } = default!;

    private Slug() { } // EF Core private constructor

    private Slug(string value)
    {
        Value = value;
    }

    public static Result<Slug> Create(string? rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(Slug), ErrorMessage = "Slug input cannot be empty."
            });
        }

        // 1. Uppercase English characters (Arabic characters are unaffected)
        string sanitized = rawInput.Trim().ToUpperInvariant();

        // 2. Normalize whitespace/underscores to hyphens
        sanitized = NormalizeWhitespaceAndUnderscoresMyRegex().Replace(sanitized, "-");

        // 3. Remove non-alphanumeric characters except hyphens & Arabic Unicode letters
        sanitized = ValidSlugRegex().Replace(sanitized, "");

        // 4. Strip duplicate consecutive hyphens and trim boundary hyphens
        sanitized = DuplicateConsecutiveHyphensMyRegex().Replace(sanitized, "-").Trim('-');

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(Slug), ErrorMessage = $"Unable to generate a valid slug from '{rawInput}'."
            });
        }

        // 5. Truncate to maximum length safely
        if (sanitized.Length > 200)
        {
            sanitized = sanitized[..200].TrimEnd('-');
        }

        return Result.Success(new Slug(sanitized));
    }

    public static implicit operator string(Slug slug) => slug.Value;

    public override string ToString() => Value;

    [GeneratedRegex(@"[\s_]+")]
    private static partial Regex NormalizeWhitespaceAndUnderscoresMyRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex DuplicateConsecutiveHyphensMyRegex();

    // Regex matches Latin letters (a-z), digits (0-9), Arabic range (\u0600-\u06FF), and single hyphens
    [GeneratedRegex(@"^[a-z0-9\u0600-\u06FF]+(-[a-z0-9\u0600-\u06FF]+)*$")]
    private static partial Regex ValidSlugRegex();
}