using FluentValidation;
using Prisma.Application.Common.Validators;

namespace Prisma.Application.Features.Authentication.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .SetValidator(new EmailValidator());

        RuleFor(x => x.Password)
            .SetValidator(new PasswordValidator());
    }
}