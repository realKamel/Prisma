using FluentValidation;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Validators;

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
            .SetValidator(new EgyptianPhoneNumberValidator());
        RuleFor(c => c.Password)
            .SetValidator(new PasswordValidator());
        RuleFor(command => command.Email)
            .SetValidator(new EmailValidator());

        RuleFor(command => command.Policies)
            .Must(permissions => permissions.Length > 0)
            .WithMessage("You must specify at least one permission.");

        RuleForEach(command => command.Policies)
            .Must(singlePolicies => AppClaims.Policies.All.Contains(singlePolicies))
            .WithMessage("Policies '{PropertyValue}' is invalid.");
    }
}