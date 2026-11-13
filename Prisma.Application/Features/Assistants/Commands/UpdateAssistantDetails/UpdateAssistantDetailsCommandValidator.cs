using FluentValidation;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Validators.ValidationExtensions;
using Prisma.Application.Features.Assistants.Commands.CreateAssistant;

namespace Prisma.Application.Features.Assistants.Commands.UpdateAssistantDetails;

public sealed class UpdateAssistantDetailsCommandValidator
    : AbstractValidator<CreateAssistantCommand>
{
    public UpdateAssistantDetailsCommandValidator()
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

        When(
            command => !string.IsNullOrWhiteSpace(command.FirstName),
            () =>
            {
                RuleFor(command => command.FirstName).MustBeValidPersonName();
            }
        );

        When(
            command => !string.IsNullOrWhiteSpace(command.SecondName),
            () =>
            {
                RuleFor(command => command.SecondName).MustBeValidPersonName();
                ;
            }
        );

        When(
            c => !string.IsNullOrWhiteSpace(c.PhoneNumber),
            () =>
            {
                RuleFor(c => c.PhoneNumber).MustBeValidPhoneNumber();
            }
        );

        When(
            c => !string.IsNullOrWhiteSpace(c.Password),
            () =>
            {
                RuleFor(c => c.Password).MustBeStrongPassword();
            }
        );

        RuleFor(command => command.Email).MustBeValidEmail();

        When(
                command => command.Policies.Count > 0,
                () =>
                {
                    RuleForEach(command => command.Policies)
                        .Must(singlePolicies => AppClaims.Policies.All.Contains(singlePolicies))
                        .WithMessage("Policies '{PropertyValue}' is invalid.");
                }
            )
            .Otherwise(() =>
            {
                RuleFor(command => command.Policies)
                    .NotEmpty()
                    .WithMessage("You must specify at least one permission.");
            });
    }
}
