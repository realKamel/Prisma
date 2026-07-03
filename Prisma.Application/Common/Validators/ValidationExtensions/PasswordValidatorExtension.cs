using System.Text.RegularExpressions;
using FluentValidation;

namespace Prisma.Application.Common.Validators.ValidationExtensions;

public static partial class PasswordValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> StrongPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        int minLength = 8,
        int maxLength = 128)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Password is required.")
            .Length(minLength, maxLength)
            .WithMessage($"Password must be between {minLength} and {maxLength} characters.")
            .Matches(UppercasePattern()).WithMessage("Password must contain at least one uppercase letter.")
            .Matches(LowercasePattern()).WithMessage("Password must contain at least one lowercase letter.")
            .Matches(DigitPattern()).WithMessage("Password must contain at least one digit.")
            .Matches(SpecialCharPattern()).WithMessage("Password must contain at least one special character.");
    }

    [GeneratedRegex("[A-Z]")]
    private static partial Regex UppercasePattern();

    [GeneratedRegex("[a-z]")]
    private static partial Regex LowercasePattern();

    [GeneratedRegex(@"\d")]
    private static partial Regex DigitPattern();

    [GeneratedRegex(@"[^A-Za-z\d]")]
    private static partial Regex SpecialCharPattern();
}