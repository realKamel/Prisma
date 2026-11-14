using FluentValidation;

namespace Prisma.Application.Features.AcademicYears.Commands.AttachTeacherToAcademicYear;

public sealed class AttachTeacherToAcademicYearCommandValidator
    : AbstractValidator<AttachTeacherToAcademicYearCommand>
{
    public AttachTeacherToAcademicYearCommandValidator()
    {
        RuleFor(x => x.TeacherId).NotNull().NotEmpty().When(x => x.TeacherId != null);
    }
}
