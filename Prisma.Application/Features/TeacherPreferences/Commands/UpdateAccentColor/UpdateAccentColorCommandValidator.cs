
using FluentValidation;

namespace Prisma.Application.Features.TeacherPreferences.Commands.UpdateAccentColor;

public sealed class UpdateAccentColorCommandValidator : AbstractValidator<UpdateAccentColorCommand>
{
    public UpdateAccentColorCommandValidator()
    {
        RuleFor(x => x.AccentColor).IsInEnum();
    }
}