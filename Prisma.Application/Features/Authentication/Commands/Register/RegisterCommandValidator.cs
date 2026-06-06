using FluentValidation;
using Prisma.Application.Common.Validators;

namespace Prisma.Application.Features.Authentication.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .SetValidator(new EmailValidator());

        RuleFor(x => x.Password)
            .SetValidator(new PasswordValidator());
    }
}