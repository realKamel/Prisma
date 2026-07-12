using FluentValidation;

namespace Prisma.Application.Common.Validators.ValidationExtensions;

public static class EmailValidatorExtension
{
    public static IRuleBuilderOptions<T, string> Email<T>(this IRuleBuilder<T, string?> ruleBuilder,
        int maxLength = 256)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Please enter a valid email address.")
            .MaximumLength(maxLength);
    }
}