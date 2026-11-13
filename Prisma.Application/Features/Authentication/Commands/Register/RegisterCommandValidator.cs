using FluentValidation;
using Prisma.Application.Common.Validators.ValidationExtensions;

namespace Prisma.Application.Features.Authentication.Commands.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(e => e.FirstName).MustBeValidPersonName();

        RuleFor(e => e.SecondName).MustBeValidPersonName();

        RuleFor(e => e.ThirdName).MustBeValidPersonName();

        RuleFor(e => e.LastName).MustBeValidPersonName();

        RuleFor(x => x.Email).MustBeValidEmail();

        RuleFor(x => x.PhoneNumber).MustBeValidPhoneNumber();

        RuleFor(x => x.Password).MustBeStrongPassword();

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Confirm Password is required.")
            .Must((model, confirm) => confirm == model.ConfirmPassword)
            .WithMessage("Passwords do not match.");

        RuleFor(x => x.ParentPhoneNumber).MustBeValidPhoneNumber();
    }
}
