using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using Prisma.Application.Common.Validators;
using Prisma.Application.Common.Validators.ValidationExtensions;

namespace Prisma.Application.Features.Students.Commands.ChangePasswordCommand;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("يجب إدخال كلمة المرور الحالية.");

        RuleFor(x => x.NewPassword)
            .StrongPassword()
            .NotEqual(x => x.CurrentPassword).WithMessage("كلمة المرور الجديدة لا يمكن أن تكون مطابقة للقديمة.");
    }
}