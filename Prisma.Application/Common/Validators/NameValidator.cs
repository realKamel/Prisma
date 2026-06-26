using System.Text.RegularExpressions;
using FluentValidation;

namespace Prisma.Application.Common.Validators;

public class PersonNameValidator : AbstractValidator<string>
{
    public PersonNameValidator()
    {
        RuleFor(x => x)
            .Must(BeValidName)
            .WithMessage("Name must contain only letters.");
    }

    private static bool BeValidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return Regex.IsMatch(name, @"^[\u0600-\u06FFa-zA-Z\s'\-.]+$");
    }
}