using System.Text.RegularExpressions;
using Ardalis.Result;

namespace Prisma.Domain.ValueObjects.UserDomain;

public sealed partial record EmailAddress
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    public string Value { get; init; } = default!;

    private EmailAddress() { } // EF Core private constructor

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static Result<EmailAddress> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(EmailAddress), ErrorMessage = "Email address cannot be empty."
            });
        }

        string normalized = email.Trim().ToUpperInvariant();

        if (normalized.Length > 255)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(EmailAddress), ErrorMessage = "Email address cannot exceed 255 characters."
            });
        }

        if (!EmailRegex().IsMatch(normalized))
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(EmailAddress), ErrorMessage = $"'{email}' is not a valid email address."
            });
        }

        return Result.Success(new EmailAddress(normalized));
    }

    public static implicit operator string(EmailAddress email)
    {
        ArgumentNullException.ThrowIfNull(email);
        return email.Value;
    }

    public override string ToString() => Value;
}