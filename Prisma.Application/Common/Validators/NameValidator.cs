using System.Text.RegularExpressions;
using FluentValidation;

namespace Prisma.Application.Common.Validators;

public partial class PersonNameValidator : AbstractValidator<string>
{
    public PersonNameValidator()
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters long.")
            .Matches(PersonNameRegex()).WithMessage("Name contains invalid characters.");
    }

    [GeneratedRegex(@"^[\u0600-\u06FFa-zA-Z\s'\-.]{2,}$")]
    private static partial Regex PersonNameRegex();
}