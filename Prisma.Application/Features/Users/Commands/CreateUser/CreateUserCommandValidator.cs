using FluentValidation;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Validators.ValidationExtensions;

namespace Prisma.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName).PersonName();
        RuleFor(x => x.SecondName).PersonName();
        RuleFor(x => x.ThirdName).PersonName();
        RuleFor(x => x.LastName).PersonName();

        RuleFor(x => x.Mobile).MustBeValidPhoneNumber();
        RuleFor(x => x.Email).Email();
        RuleFor(x => x.Password).StrongPassword();

        RuleFor(x => x.Role)
            .Must(r => AppRoles.RolesSet.Contains(r))
            .WithMessage("Role must be one of Admin, Teacher, Student, Assistant.");

        When(
            x => x.Role == AppRoles.Student,
            () =>
            {
                RuleFor(x => x.GradeId).NotNull().WithMessage("Grade is required for students.");
                RuleFor(x => x.TeacherId)
                    .NotNull()
                    .WithMessage("Teacher is required for students.");
                RuleFor(x => x.ParentMobile).NotEmpty().EgyptianPhoneNumber();
                RuleFor(x => x)
                    .Must(x => x.Mobile != x.ParentMobile)
                    .WithMessage("Student phone and parent phone cannot be the same.");
            }
        );
    }
}
