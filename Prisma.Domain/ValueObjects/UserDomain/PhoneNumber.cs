using System.Text.RegularExpressions;
using Ardalis.Result;

namespace Prisma.Domain.ValueObjects.UserDomain;

public sealed partial record PhoneNumber
{
    [GeneratedRegex(@"^\+[1-9]\d{1,14}$")]
    private static partial Regex E164Regex();

    public string Value { get; init; } = default!;

    // EF Core parameterless constructor (required for materialization)
    private PhoneNumber() { }

    // Domain constructor
    private PhoneNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Factory method returning Result
    /// </summary>
    public static Result<PhoneNumber> Create(string? rawNumber)
    {
        if (string.IsNullOrWhiteSpace(rawNumber))
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(PhoneNumber), ErrorMessage = "Phone number cannot be empty."
            });
        }

        // Sanitize spaces, dashes, and parentheses
        string sanitized = MyRegex().Replace(rawNumber, "");

        if (!E164Regex().IsMatch(sanitized))
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(PhoneNumber),
                ErrorMessage = $"'{rawNumber}' is not a valid E.164 phone number (e.g. +14155552671)."
            });
        }

        return Result.Success(new PhoneNumber(sanitized));
    }

    public static implicit operator string(PhoneNumber number)
    {
        ArgumentNullException.ThrowIfNull(number);
        return number.Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"[\s\-\(\)]")]
    private static partial Regex MyRegex();
}