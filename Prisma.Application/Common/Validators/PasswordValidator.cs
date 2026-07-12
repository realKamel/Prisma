using System.Text.RegularExpressions;
using FluentValidation;

namespace Prisma.Application.Common.Validators;

public class PasswordValidator : AbstractValidator<string>
{
    public PasswordValidator()
    {
        RuleFor(x => x)
            .Must(BeStrongPassword)
            .WithMessage("Password must contain uppercase, lowercase, digit, and special character.");
    }

    private static bool BeStrongPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8 || password.Length > 128)
            return false;
        if (password.Contains(' ')) return false;
        if (!Regex.IsMatch(password, "[A-Z]")) return false;
        if (!Regex.IsMatch(password, "[a-z]")) return false;
        //if (!Regex.IsMatch(password, @"\d")) return false;
        if (!Regex.IsMatch(password, "[!@#$%^&*()\\-_+=[\\]{};':\"|,.<>/?]")) return false;
        return true;
    }
}