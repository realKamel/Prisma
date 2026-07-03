using FluentValidation;
using Prisma.Application.Common.Validators.ValidationExtensions;

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
                .Email();
        });

        When(x => !string.IsNullOrWhiteSpace(x.Phone), () =>
        {
            RuleFor(x => x.Phone)
                .EgyptianPhoneNumber();
        });

        RuleFor(x => x.Password)
            .StrongPassword();
    }

    private static bool HaveEmailOrPhone(LoginCommand command)
    {
        return !string.IsNullOrWhiteSpace(command.Email)
               || !string.IsNullOrWhiteSpace(command.Phone);
    }
}