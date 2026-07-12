using FluentValidation;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Validators;
using Prisma.Application.Common.Validators.ValidationExtensions;

namespace Prisma.Application.Features.Assistants.Commands.CreateAssistant;

public class CreateAssistantCommandValidator : AbstractValidator<CreateAssistantCommand>
{
    public CreateAssistantCommandValidator()
    {
        RuleFor(command => command.FirstName)
            .SetValidator(new PersonNameValidator());
        RuleFor(command => command.LastName)
            .SetValidator(new PersonNameValidator());
        RuleFor(c => c.PhoneNumber)
            .EgyptianPhoneNumber();
        RuleFor(c => c.Password)
            .StrongPassword();
        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email is invalid.");

        RuleFor(command => command.Policies)
            .Must(permissions => permissions.Length > 0)
            .WithMessage("You must specify at least one permission.");

        RuleForEach(command => command.Policies)
            .Must(singlePolicies => AppClaims.Policies.All.Contains(singlePolicies))
            .WithMessage("Policies '{PropertyValue}' is invalid.");
    }
}