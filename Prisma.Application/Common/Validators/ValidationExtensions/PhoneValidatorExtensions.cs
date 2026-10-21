using System.Text.RegularExpressions;
using FluentValidation;
using PhoneNumbers;
using Prisma.Domain.Errors;

namespace Prisma.Application.Common.Validators.ValidationExtensions;

public static partial class PhoneValidatorExtensions
{
    private static readonly PhoneNumberUtil PhoneUtil = PhoneNumberUtil.GetInstance();

    public static IRuleBuilderOptions<T, string> EgyptianPhoneNumber<T>(
        this IRuleBuilder<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Matches(EgyptianPhoneRegex)
            .WithMessage("Invalid Egyptian phone number.");
    }

    public static IRuleBuilderOptions<T, string?> E164PhoneNumber<T>(
        this IRuleBuilder<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .WithErrorCode(DomainErrors.AuthenticationErrors.PhoneNumberRequired);
        // .WithErrorCode(DomainErrors.AuthenticationErrors.InvalidPhoneNumber);
    }

    /// <summary>
    /// Validates that a string is a valid international or regional phone number using Google's libphonenumber.
    /// </summary>
    /// <param name="defaultRegion">Two-letter ISO region code (e.g., "EG", "US") used if numbers are input in local format.</param>
    public static IRuleBuilderOptions<T, string?> MustBeValidPhoneNumber<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string defaultRegion = "EG"
    )
    {
        return ruleBuilder
            .Must(phoneNumber =>
            {
                // // Allow null/empty if the property isn't explicitly [Required]
                // if (string.IsNullOrWhiteSpace(phoneNumber))
                // {
                //     return true;
                // }

                try
                {
                    var parsedNumber = PhoneUtil.Parse(phoneNumber, defaultRegion);
                    return PhoneUtil.IsValidNumber(parsedNumber);
                }
                catch (NumberParseException)
                {
                    return false;
                }
            })
            .WithMessage("'{PropertyName}' is not a valid phone number.");
    }

    [GeneratedRegex(@"^(\+20|0)1[0125]\d{8}$")]
    private static partial Regex MyRegex();

    private static readonly Regex EgyptianPhoneRegex = MyRegex();
}
