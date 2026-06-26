using FluentValidation;
using Prisma.Application.Common.Validators;

namespace Prisma.Application.Features.TeacherStudents.Commands.AddStudent;

public class AddStudentCommandValidator : AbstractValidator<AddStudentCommand>
{
    public AddStudentCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().MinimumLength(2).MaximumLength(20)
            .SetValidator(new PersonNameValidator());

        RuleFor(x => x.SecondName)
            .NotEmpty().MinimumLength(2).MaximumLength(20)
            .SetValidator(new PersonNameValidator());

        RuleFor(x => x.ThirdName)
            .NotEmpty().MinimumLength(2).MaximumLength(20)
            .SetValidator(new PersonNameValidator());

        RuleFor(x => x.LastName)
            .NotEmpty().MinimumLength(2).MaximumLength(20)
            .SetValidator(new PersonNameValidator());

        RuleFor(x => x.Mobile)
            .NotEmpty()
            .SetValidator(new EgyptianPhoneNumberValidator());

        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress()
            .SetValidator(new EmailValidator());

        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(8).MaximumLength(128)
            .SetValidator(new PasswordValidator());

        RuleFor(x => x.Grade)
            .NotEmpty().WithMessage("Grade is required.");

        RuleFor(x => x.ParentMobile)
            .NotEmpty()
            .SetValidator(new EgyptianPhoneNumberValidator());

        RuleFor(x => x)
            .Must(x => x.Mobile != x.ParentMobile)
            .WithMessage("Student phone and parent phone cannot be the same.");
    }
}