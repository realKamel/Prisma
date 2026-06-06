using FluentValidation;

namespace Prisma.Application.Common.Validators;

public class EmailValidator : AbstractValidator<string>
{
    public EmailValidator()
    {
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Email is required.")
            .MaximumLength(254)
            .WithMessage("Email must not exceed 254 characters.")
            .EmailAddress()
            .WithMessage("Email format is invalid.")
            .Must(email => !email.StartsWith('.') && !email.EndsWith('.'))
            .WithMessage("Email must not start or end with a dot.")
            .Must(email => !email.Contains(".."))
            .WithMessage("Email must not contain consecutive dots.")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
            .WithMessage("Email must have a valid domain.");
    }
}