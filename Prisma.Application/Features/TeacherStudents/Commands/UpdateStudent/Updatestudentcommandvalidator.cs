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

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);

        RuleFor(x => x.Grade).GreaterThan(0);

        // Password is optional in edit — only validate when provided
        RuleFor(x => x.NewPassword)
            .MinimumLength(8)
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.")
            .When(x => !string.IsNullOrWhiteSpace(x.NewPassword));

        RuleFor(x => x.ParentMobile)
            .Matches(PhoneRe).WithMessage("Parent mobile must be a valid Egyptian phone number.")
            .When(x => !string.IsNullOrEmpty(x.ParentMobile));
    }
}