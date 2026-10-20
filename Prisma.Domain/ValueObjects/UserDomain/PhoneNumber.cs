using Ardalis.Result;
using PhoneNumbers;
using Prisma.Domain.Errors;

namespace Prisma.Domain.ValueObjects.UserDomain;

public sealed record PhoneNumber
{
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
    public static Result<PhoneNumber> Create(string? rawNumber, string defaultRegion = "EG")
    {
        if (string.IsNullOrWhiteSpace(rawNumber))
        {
            return Result.Invalid(
                new ValidationError
                {
                    Identifier = nameof(PhoneNumber),
                    ErrorMessage = "Phone number cannot be empty.",
                    ErrorCode = DomainErrors.DomainValdtionErrors.PhoneNumber.IsRequired,
                }
            );
        }

        try
        {
            var phoneUtil = PhoneNumberUtil.GetInstance();

            // Parse using Google's library (handles local formats like 010xxx too if defaultRegion is provided)
            var parsedNumber = phoneUtil.Parse(rawNumber, defaultRegion);

            if (!phoneUtil.IsValidNumber(parsedNumber))
            {
                return Result.Invalid(
                    new ValidationError
                    {
                        Identifier = nameof(PhoneNumber),
                        ErrorMessage = $"'{rawNumber}' is not a valid phone number.",
                        ErrorCode = DomainErrors.DomainValdtionErrors.PhoneNumber.Invalid,
                    }
                );
            }

            // Automatically normalize to strict E.164 format (e.g., +201001234567)
            string e164Normalized = phoneUtil.Format(parsedNumber, PhoneNumberFormat.E164);

            return Result.Success(new PhoneNumber(e164Normalized));
        }
        catch (NumberParseException)
        {
            return Result.Invalid(
                new ValidationError
                {
                    Identifier = nameof(PhoneNumber),
                    ErrorMessage = $"'{rawNumber}' is not a valid phone number format.",
                    ErrorCode = DomainErrors.DomainValdtionErrors.PhoneNumber.Invalid,
                }
            );
        }
    }

    public static implicit operator string(PhoneNumber number)
    {
        ArgumentNullException.ThrowIfNull(number);
        return number.Value;
    }

    public override string ToString() => Value;
}
