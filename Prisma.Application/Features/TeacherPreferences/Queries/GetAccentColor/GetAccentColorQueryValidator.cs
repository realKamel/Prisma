using FluentValidation;

namespace Prisma.Application.Features.TeacherPreferences.Queries.GetAccentColor;

public sealed class GetAccentColorQueryValidator : AbstractValidator<GetAccentColorQuery>
{
    public GetAccentColorQueryValidator()
    {
        RuleFor(x => x.TeacherEmail)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة");
    }
}
