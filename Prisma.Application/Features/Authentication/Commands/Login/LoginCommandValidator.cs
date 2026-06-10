using FluentValidation;
using Prisma.Application.Common.Validators;

namespace Prisma.Application.Features.Authentication.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x)
            .Must(HaveEmailOrPhone)
            .WithMessage("Either Email or Phone must be provided.");

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .SetValidator(new EmailValidator());
        });

        When(x => !string.IsNullOrWhiteSpace(x.Phone), () =>
        {
            RuleFor(x => x.Phone)
                .SetValidator(new EgyptianPhoneNumberValidator());
        });

        RuleFor(x => x.Password)
            .SetValidator(new PasswordValidator());
    }

    private static bool HaveEmailOrPhone(LoginCommand command)
    {
        return !string.IsNullOrWhiteSpace(command.Email)
               || !string.IsNullOrWhiteSpace(command.Phone);
    }
}