using System.Text.RegularExpressions;
using FluentValidation;

namespace Prisma.Application.Common.Validators.ValidationExtensions;

public static partial class PersonNameValidatorExtension
{
    public static IRuleBuilderOptions<T, string?> PersonName<T>(
        this IRuleBuilder<T, string?> ruleBuilder
    )
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Name is required.")
            .MinimumLength(2)
            .WithMessage("Name must be at least 2 characters long.")
            .Matches(PersonNameRegex())
            .WithMessage("Name contains invalid characters.");
    }

    [GeneratedRegex(@"^[\u0600-\u06FFa-zA-Z\s'\-.]{2,}$")]
    private static partial Regex PersonNameRegex();
}
