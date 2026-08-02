using FluentValidation;
using Prisma.Application.Common.Validators;
using Prisma.Application.Common.Validators.ValidationExtensions;

namespace Prisma.Application.Features.Authentication.Commands.EmailVerification;

public class EmailVerificationRequestCommandValidator : AbstractValidator<EmailVerificationRequestCommand>
{
    public EmailVerificationRequestCommandValidator()
    {
        RuleFor(x => x.Email)
            .Email();
    }
}

public class ConfirmEmailCommandValidation : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidation()
    {
        RuleFor(x => x.Email)
            .Email();
    }
}