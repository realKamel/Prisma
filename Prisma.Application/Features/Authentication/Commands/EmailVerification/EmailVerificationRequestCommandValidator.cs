using FluentValidation;
using Prisma.Application.Common.Validators.ValidationExtensions;

namespace Prisma.Application.Features.Authentication.Commands.EmailVerification;

public sealed class EmailVerificationRequestCommandValidator
    : AbstractValidator<EmailVerificationRequestCommand>
{
    public EmailVerificationRequestCommandValidator()
    {
        RuleFor(x => x.Email).MustBeValidEmail();
    }
}

public sealed class ConfirmEmailCommandValidation : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidation()
    {
        RuleFor(x => x.Email).MustBeValidEmail();
    }
}
