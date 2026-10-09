using FluentValidation;
using Prisma.Domain.Errors;

namespace Prisma.Application.Common.Validators.ValidationExtensions;

public static class EmailValidatorExtension
{
    public static IRuleBuilderOptions<T, string> Email<T>(this IRuleBuilder<T, string?> ruleBuilder,
        int maxLength = 256)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Email is required.")
            .WithErrorCode(DomainErrors.AuthenticationErrors.EmailAddressIsRequired)
            .EmailAddress()
            .WithMessage("Please enter a valid email address.")
            .MaximumLength(maxLength);
    }
}