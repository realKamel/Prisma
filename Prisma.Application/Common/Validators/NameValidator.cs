using FluentValidation;

namespace Prisma.Application.Common.Validators;

public class PersonNameValidator : AbstractValidator<string>
{
    public PersonNameValidator()
    {
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MinimumLength(2)
            .WithMessage("Name must be at least 2 characters.")
            .MaximumLength(20)
            .WithMessage("Name must not exceed 100 characters.")
            .Matches(@"^[\p{L}\s'\-\.]+$")
            .WithMessage("Name can only contain letters, spaces, hyphens, apostrophes, and dots.");
    }
}