using FluentValidation;
using Prisma.Application.Common.Validators;

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
            .SetValidator(new EmailValidator());

        RuleFor(x => x.PhoneNumber)
            .SetValidator(new EgyptianPhoneNumberValidator());

        RuleFor(x => x.Password)
            .SetValidator(new PasswordValidator());

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .Must((model, confirm) => confirm == model.ConfirmPassword)
            .WithMessage("Passwords do not match.");

        RuleFor(x => x.ParentPhoneNumber)
            .SetValidator(new EgyptianPhoneNumberValidator());
    }
}