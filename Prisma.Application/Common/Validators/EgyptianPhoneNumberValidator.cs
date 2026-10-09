using System.Text.RegularExpressions;
using FluentValidation;

namespace Prisma.Application.Common.Validators;

public partial class EgyptianPhoneNumberValidator : AbstractValidator<string>
{
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

    [GeneratedRegex(@"^(\+20|0)1[0125]\d{8}$")]
    private static partial Regex MyRegex();

    private static readonly Regex PhoneRegex = MyRegex();
}