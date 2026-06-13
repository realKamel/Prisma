using FluentValidation;
using Prisma.Application.Common.Validators;
using Prisma.Application.Features.Authentication.Commands.ForgotPassword;

namespace Prisma.Application.Features.Authentication.Commands.EmailVerification;

public class EmailVerificationRequestCommandValidation : AbstractValidator<EmailVerificationRequestCommand>
{
    public EmailVerificationRequestCommandValidation()
    {
        RuleFor(x => x.Email)
            .SetValidator(new EmailValidator());
    }
}
public class ConfirmEmailCommandValidation : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidation()
    {
        RuleFor(x => x.Email)
            .SetValidator(new EmailValidator());    }
}
