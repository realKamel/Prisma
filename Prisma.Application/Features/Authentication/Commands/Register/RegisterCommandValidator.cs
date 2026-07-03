using FluentValidation;
using Prisma.Application.Common.Validators;
using Prisma.Application.Common.Validators.ValidationExtensions;

namespace Prisma.Application.Features.Authentication.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(e => e.FirstName)
            .SetValidator(new PersonNameValidator());

        RuleFor(e => e.SecondName)
            .SetValidator(new PersonNameValidator());

        RuleFor(e => e.ThirdName)
            .SetValidator(new PersonNameValidator());

        RuleFor(e => e.LastName)
            .SetValidator(new PersonNameValidator());

        RuleFor(x => x.Email)
            .Email();

        RuleFor(x => x.PhoneNumber)
            .EgyptianPhoneNumber();

        RuleFor(x => x.Password)
            .StrongPassword();

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Confirm Password is required.")
            .Must((model, confirm) => confirm == model.ConfirmPassword)
            .WithMessage("Passwords do not match.");

        RuleFor(x => x.ParentPhoneNumber)
            .EgyptianPhoneNumber();
    }
}