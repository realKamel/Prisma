using FluentValidation;
using Prisma.Application.Common.Constants;

namespace Prisma.Application.Features.Assistants.Commands.UpdatePermissions;

public class UpdatePermissionCommandValidator : AbstractValidator<UpdatePermissionCommand>
{
    public UpdatePermissionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required.");

        RuleForEach(command => command.Permission)
            .Must(singlePolicies => AppClaims.Policies.All.Contains(singlePolicies))
            .WithMessage("Policies '{PropertyValue}' is invalid.");
    }
}