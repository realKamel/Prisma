using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using Prisma.Application.Common.Validators;
using Prisma.Application.Common.Validators.ValidationExtensions;

namespace Prisma.Application.Features.Authentication.Commands.ForgotPassword;

public class ForgetPasswordCommandValidation : AbstractValidator<ForgotPasswordCommand>
{
    public ForgetPasswordCommandValidation()
    {
        RuleFor(x => x.Email)
            .Email();
    }
}

public class ConfirmCodeCommandValidation : AbstractValidator<ConfirmCodeCommand>
{
    public ConfirmCodeCommandValidation()
    {
        RuleFor(x => x.Email)
            .Email();

        RuleFor(x => x.Code)
            .Length(6).WithMessage("Code Invalid");
    }
}

public class ResetPasswordCommandValidation : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidation()
    {
        RuleFor(x => x.Email)
            .Email();

        RuleFor(x => x.NewPassword)
            .StrongPassword();
    }
}