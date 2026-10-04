using Ardalis.Result;

namespace Prisma.Domain.ValueObjects.AssessmentDomain;

public sealed record OptionKey
{
    public char Value { get; init; }

    private OptionKey() { } // EF Core private constructor

    private OptionKey(char value)
    {
        Value = value;
    }

    public static Result<OptionKey> Create(char key)
    {
        char uppercaseKey = char.ToUpperInvariant(key);

        if (uppercaseKey is < 'A' or > 'Z')
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(OptionKey),
                ErrorMessage = $"Option key must be a single uppercase letter (A-Z). Got: '{key}'."
            });
        }

        return new OptionKey(uppercaseKey);
    }

    /// <summary>
    /// Creates an OptionKey from a zero-based index (0 for A, 1 for B, ..., 25 for Z).
    /// </summary>
    /// <param name="zeroBasedIndex">zeroBasedIndex</param>
    /// <returns>Ardalis.Result</returns>
    public static Result<OptionKey> FromIndex(int zeroBasedIndex)
    {
        if (zeroBasedIndex is < 0 or > 25)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(OptionKey), ErrorMessage = "Option index must be between 0 (A) and 25 (Z)."
            });
        }

        return Create((char)('A' + zeroBasedIndex));
    }

    public static implicit operator char(OptionKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return key.Value;
    }

    public static char ToChar(OptionKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return key.Value;
    }

    public override string ToString() => Value.ToString();
}