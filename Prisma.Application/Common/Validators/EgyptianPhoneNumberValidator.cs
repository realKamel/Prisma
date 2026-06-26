using System.Text.RegularExpressions;
using FluentValidation;

namespace Prisma.Application.Common.Validators;

public class EgyptianPhoneNumberValidator : AbstractValidator<string>
{
    private static readonly Regex PhoneRegex = new(@"^(010|011|012|015)\d{8}$");

    public EgyptianPhoneNumberValidator()
    {
        RuleFor(x => x)
            .Must(BeValidEgyptianPhone)
            .WithMessage("Invalid Egyptian phone number.");
    }

    private static bool BeValidEgyptianPhone(string phone)
    {
        return !string.IsNullOrWhiteSpace(phone) && PhoneRegex.IsMatch(phone);
    }
}