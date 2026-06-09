using FluentValidation;
using Prisma.Application.Common.Validators;

namespace Prisma.Application.Features.Authentication.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x)
            .Must(HaveEmailOrPhone)
            .WithName(nameof(LoginCommand.Phone))
            .WithMessage("Either Email or Phone must be provided.");

        RuleFor(x => x.Password)
            .SetValidator(new PasswordValidator());
    }

    private static bool HaveEmailOrPhone(LoginCommand command)
    {
        return !string.IsNullOrWhiteSpace(command.Email)
               || !string.IsNullOrWhiteSpace(command.Phone);
    }
}