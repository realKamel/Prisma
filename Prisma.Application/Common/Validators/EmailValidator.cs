using FluentValidation;

namespace Prisma.Application.Common.Validators;

public class EmailValidator : AbstractValidator<string?>
{
    public EmailValidator()
    {
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("{PropertyName} is required.")
            .MaximumLength(254)
            .WithMessage("{PropertyName} must not exceed 254 characters.")
            .EmailAddress()
            .WithMessage("{PropertyName} format is invalid.")
            .Must(email => !email.StartsWith('.') && !email.EndsWith('.'))
            .WithMessage("{PropertyName} must not start or end with a dot.")
            .Must(email => !email.Contains(".."))
            .WithMessage("{PropertyName} must not contain consecutive dots.")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
            .WithMessage("{PropertyName} must have a valid domain.");
    }
}