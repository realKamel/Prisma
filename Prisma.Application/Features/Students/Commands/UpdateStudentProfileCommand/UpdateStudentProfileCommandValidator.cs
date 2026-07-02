using FluentValidation;

namespace Prisma.Application.Features.Students.Commands.UpdateStudentProfileCommand;

public sealed class UpdateStudentProfileCommandValidator : AbstractValidator<UpdateStudentProfileCommand>
{
    public UpdateStudentProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("الاسم الأول مطلوب.");
        RuleFor(x => x.secondName)
            .NotEmpty().WithMessage("الاسم الثاني مطلوب.");
        RuleFor(x => x.thirdName)
            .NotEmpty().WithMessage("الاسم الثالث مطلوب.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("اسم العائلة مطلوب.");

        RuleFor(x => x.Mobile)
            .NotEmpty().WithMessage("رقم الموبايل مطلوب.")
            .Matches(@"^01[0125]\d{8}$").WithMessage("رقم الموبايل غير صحيح.");

        RuleFor(x => x.AcademicYearId)
            .GreaterThan(0).WithMessage("يجب اختيار الصف الدراسي بشكل صحيح.");

        RuleFor(x => x.ParentMobile)
            .NotEmpty().WithMessage("رقم واتساب ولي الأمر مطلوب.")
            .Matches(@"^01[0125]\d{8}$").WithMessage("رقم ولي الأمر غير صحيح.");
    }
}