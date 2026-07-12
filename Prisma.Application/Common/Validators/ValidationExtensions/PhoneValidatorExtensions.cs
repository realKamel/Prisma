using System.Text.RegularExpressions;
using FluentValidation;

namespace Prisma.Application.Common.Validators.ValidationExtensions;

public static partial class PhoneValidatorExtensions
{
    private static readonly Regex EgyptianPhoneRegex = MyRegex();

    public static IRuleBuilderOptions<T, string> EgyptianPhoneNumber<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Matches(EgyptianPhoneRegex)
            .WithMessage("Invalid Egyptian phone number.");
    }

    [GeneratedRegex(@"^(\+20|0)1[0125]\d{8}$")]
    private static partial Regex MyRegex();
}