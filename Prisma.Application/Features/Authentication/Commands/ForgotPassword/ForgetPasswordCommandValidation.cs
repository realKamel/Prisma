using FluentValidation;
using Prisma.Application.Common.Validators.ValidationExtensions;

namespace Prisma.Application.Features.Authentication.Commands.ForgotPassword;

public sealed class ForgetPasswordCommandValidation : AbstractValidator<ForgotPasswordCommand>
{
    public ForgetPasswordCommandValidation()
    {
        RuleFor(x => x.Email).MustBeValidEmail();
    }
}

public sealed class ConfirmCodeCommandValidation : AbstractValidator<ConfirmCodeCommand>
{
    public ConfirmCodeCommandValidation()
    {
        RuleFor(x => x.Email).MustBeValidEmail();

        RuleFor(x => x.Code).Length(6).WithMessage("Code Invalid");
    }
}

public sealed class ResetPasswordCommandValidation : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidation()
    {
        RuleFor(x => x.Email).MustBeValidEmail();

        RuleFor(x => x.NewPassword).MustBeStrongPassword();
    }
}
