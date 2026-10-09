using System.Text.RegularExpressions;
using FluentValidation;
using Prisma.Domain.Errors;

namespace Prisma.Application.Common.Validators.ValidationExtensions;

public static partial class PhoneValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> EgyptianPhoneNumber<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Matches(EgyptianPhoneRegex)
            .WithMessage("Invalid Egyptian phone number.");
    }

    public static IRuleBuilderOptions<T, string?> E164PhoneNumber<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .WithErrorCode(DomainErrors.AuthenticationErrors.InvalidPhoneNumber);
    }

    [GeneratedRegex(@"^(\+20|0)1[0125]\d{8}$")]
    private static partial Regex MyRegex();

    private static readonly Regex EgyptianPhoneRegex = MyRegex();
}