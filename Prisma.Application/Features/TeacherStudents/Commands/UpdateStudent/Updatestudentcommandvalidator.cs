using FluentValidation;

namespace Prisma.Application.Features.TeacherStudents.Commands.UpdateStudent;

public class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
{
    private static readonly System.Text.RegularExpressions.Regex PhoneRe =
        new(@"^(010|011|012|015)\d{8}$");

    public UpdateStudentCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();

        RuleFor(x => x.FirstName).NotEmpty().MinimumLength(2).MaximumLength(20);
        RuleFor(x => x.SecondName).NotEmpty().MinimumLength(2).MaximumLength(20);
        RuleFor(x => x.ThirdName).NotEmpty().MinimumLength(2).MaximumLength(20);
        RuleFor(x => x.LastName).NotEmpty().MinimumLength(2).MaximumLength(20);

        RuleFor(x => x.Mobile)
            .NotEmpty()
            .Matches(PhoneRe).WithMessage("Mobile must be a valid Egyptian phone number.");

        RuleFor(x => x.Grade).GreaterThan(0);

        RuleFor(x => x.ParentMobile)
            .Matches(PhoneRe).WithMessage("Parent mobile must be a valid Egyptian phone number.")
            .When(x => !string.IsNullOrEmpty(x.ParentMobile));
    }
}