using FluentValidation;

namespace Prisma.Application.Common.Validators;

public class PersonNameValidator : AbstractValidator<string>
{
    public PersonNameValidator()
    {
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("{PropertyName} is required.")
            .MinimumLength(2)
            .WithMessage("{PropertyName} must be at least 2 characters.")
            .MaximumLength(20)
            .WithMessage("{PropertyName} must not exceed 100 characters.")
            .Matches(@"^[\p{L}\s'\-\.]+$")
            .WithMessage("{PropertyName} can only contain letters, spaces, hyphens, apostrophes, and dots.");
    }
}