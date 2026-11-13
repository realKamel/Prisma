using FluentValidation;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Validators.ValidationExtensions;

namespace Prisma.Application.Features.Assistants.Commands.CreateAssistant;

public sealed class CreateAssistantCommandValidator : AbstractValidator<CreateAssistantCommand>
{
    public CreateAssistantCommandValidator()
    {
        RuleFor(command => command.FirstName).MustBeValidPersonName();
        RuleFor(command => command.SecondName).MustBeValidPersonName();
        RuleFor(c => c.PhoneNumber).MustBeValidPhoneNumber();
        RuleFor(c => c.Password).MustBeStrongPassword();
        RuleFor(command => command.Email).MustBeValidEmail();

        RuleFor(command => command.Policies)
            .Must(permissions => permissions.Count > 0)
            .WithMessage("You must specify at least one permission.");

        RuleForEach(command => command.Policies)
            .Must(singlePolicies => AppClaims.Policies.All.Contains(singlePolicies))
            .WithMessage("Policies '{PropertyValue}' is invalid.");
    }
}
