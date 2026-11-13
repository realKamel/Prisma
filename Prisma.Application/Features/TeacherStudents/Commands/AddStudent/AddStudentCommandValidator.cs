using FluentValidation;
using Prisma.Application.Common.Validators.ValidationExtensions;

namespace Prisma.Application.Features.TeacherStudents.Commands.AddStudent;

public sealed class AddStudentCommandValidator : AbstractValidator<AddStudentCommand>
{
    public AddStudentCommandValidator()
    {
        RuleFor(x => x.FirstName).MustBeValidPersonName();
        RuleFor(x => x.SecondName).MustBeValidPersonName();
        RuleFor(x => x.ThirdName).MustBeValidPersonName();
        RuleFor(x => x.LastName).MustBeValidPersonName();

        RuleFor(x => x.Mobile).NotEmpty().MustBeValidPhoneNumber();

        RuleFor(x => x.Email).NotEmpty().MustBeValidEmail();

        RuleFor(x => x.Password).MustBeStrongPassword();

        RuleFor(x => x.Grade).NotEmpty().WithMessage("Grade is required.");

        RuleFor(x => x.ParentMobile).MustBeValidPhoneNumber();

        RuleFor(x => x)
            .Must(x => x.Mobile != x.ParentMobile)
            .WithMessage("Student phone and parent phone cannot be the same.");
    }
}
