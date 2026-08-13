using FluentValidation;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Validators;
using Prisma.Application.Common.Validators.ValidationExtensions;
using Prisma.Application.Features.Assistants.Commands.CreateAssistant;

namespace Prisma.Application.Features.Assistants.Commands.UpdateAssistantDetails;

public class UpdateAssistantDetailsCommandValidator : AbstractValidator<CreateAssistantCommand>
{
    public UpdateAssistantDetailsCommandValidator()
    {
        RuleFor(command => command.FirstName)
            .SetValidator(new PersonNameValidator());
        RuleFor(command => command.SecondName)
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


        When(command => !string.IsNullOrWhiteSpace(command.FirstName), () =>
        {
            RuleFor(command => command.FirstName)
                .SetValidator(new PersonNameValidator());
        });

        When(command => !string.IsNullOrWhiteSpace(command.SecondName), () =>
        {
            RuleFor(command => command.SecondName)
                .SetValidator(new PersonNameValidator());
        });

        When(c => !string.IsNullOrWhiteSpace(c.PhoneNumber), () =>
        {
            RuleFor(c => c.PhoneNumber)
                .EgyptianPhoneNumber();
        });

        When(c => !string.IsNullOrWhiteSpace(c.Password), () =>
        {
            RuleFor(c => c.Password)
                .StrongPassword();
        });

        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is invalid.");

        When(command => command.Policies is not null && command.Policies.Length > 0, () =>
        {
            RuleForEach(command => command.Policies)
                .Must(singlePolicies => AppClaims.Policies.All.Contains(singlePolicies))
                .WithMessage("Policies '{PropertyValue}' is invalid.");
        })
        .Otherwise(() =>
        {
            RuleFor(command => command.Policies)
                .NotEmpty().WithMessage("You must specify at least one permission.");
        });
    }
}