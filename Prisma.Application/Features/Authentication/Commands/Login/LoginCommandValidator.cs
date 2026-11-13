using FluentValidation;
using Prisma.Application.Common.Validators.ValidationExtensions;
using Prisma.Domain.Errors;

namespace Prisma.Application.Features.Authentication.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x)
            .Must(HaveEmailOrPhone)
            .WithErrorCode(DomainErrors.AuthenticationErrors.MustProvideEmailOrPhone)
            .WithMessage("Either Email or Phone must be provided.");

        When(
            x => !string.IsNullOrWhiteSpace(x.Email),
            () =>
            {
                RuleFor(x => x.Email).MustBeValidEmail();
            }
        );

        When(
            x => !string.IsNullOrWhiteSpace(x.Phone),
            () =>
            {
                RuleFor(x => x.Phone).MustBeValidPhoneNumber();
            }
        );

        RuleFor(x => x.Password).MustBeStrongPassword();
    }

    private static bool HaveEmailOrPhone(LoginCommand command)
    {
        return !string.IsNullOrWhiteSpace(command.Email)
            || !string.IsNullOrWhiteSpace(command.Phone);
    }
}
