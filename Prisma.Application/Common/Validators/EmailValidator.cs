using System.Text.RegularExpressions;
using FluentValidation;

namespace Prisma.Application.Common.Validators;

public class EmailValidator : AbstractValidator<string>
{
    public EmailValidator()
    {
        RuleFor(x => x)
            .Must(BeGmail)
            .WithMessage("Email must be a valid Gmail address.");
    }

    private static bool BeGmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        var e = email.Trim().ToLower();
        return e.EndsWith("@gmail.com") &&
               Regex.IsMatch(e, @"^[^\s@]+@gmail\.com$") &&
               !e.StartsWith(".") &&
               !e.Split('@')[0].EndsWith(".") &&
               !e.Split('@')[0].Contains("..");
    }
}