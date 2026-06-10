using FluentValidation;

namespace Prisma.Application.Common.Validators;

public class EgyptianPhoneNumberValidator : AbstractValidator<string?>
{
    public EgyptianPhoneNumberValidator()
    {
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("{PropertyName} is required.")
            .Matches(@"^01[0125][0-9]{8}$")
            .WithMessage("Phone number must be in correct format (e.g., 01123456789).");
    }
}