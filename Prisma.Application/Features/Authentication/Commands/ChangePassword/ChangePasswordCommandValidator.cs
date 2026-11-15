using FluentValidation;

namespace Prisma.Application.Features.Authentication.Commands.ChangePassword;

/// <summary>Same rules, order and message keys as the Angular password-rules.ts.</summary>
public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("VALIDATION.REQUIRED");

        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("VALIDATION.REQUIRED")
            .Must(v => v.Length >= 8)
            .WithMessage("VALIDATION.PASSWORD_MIN_LENGTH")
            .Must(v => v.Length <= 128)
            .WithMessage("VALIDATION.PASSWORD_MAX_LENGTH")
            .Must(v => !v.Any(char.IsWhiteSpace))
            .WithMessage("VALIDATION.PASSWORD_NO_SPACES")
            .Must(v => v.Any(c => c is >= 'A' and <= 'Z'))
            .WithMessage("VALIDATION.PASSWORD_UPPERCASE")
            .Must(v => v.Any(c => c is >= 'a' and <= 'z'))
            .WithMessage("VALIDATION.PASSWORD_LOWERCASE")
            .Must(v => v.Any(c => c is >= '0' and <= '9'))
            .WithMessage("VALIDATION.PASSWORD_DIGIT")
            .Must(v => v.Any(c => !char.IsAsciiLetterOrDigit(c) && !char.IsWhiteSpace(c)))
            .WithMessage("VALIDATION.PASSWORD_SPECIAL")
            .Must((command, v) => v != command.CurrentPassword)
            .WithMessage("TEACHER_PROFILE_PAGE.ACCOUNT.SAME_AS_OLD");
    }
}
