using FluentValidation;
using Prisma.Application.Common.Validators.ValidationExtensions;
using Prisma.Domain.Errors;

namespace Prisma.Application.Features.TeacherOnboarding.Commands;

public class OnboardTeacherCommandValidator : AbstractValidator<OnboardTeacherCommand>
{
    public OnboardTeacherCommandValidator()
    {
        RuleFor(x => x.FirstName).PersonName();
        RuleFor(x => x.SecondName).PersonName();
        RuleFor(x => x.ThirdName).PersonName();
        RuleFor(x => x.LastName).PersonName();
        RuleFor(x => x.PhoneNumber).E164PhoneNumber();
        RuleFor(x => x.Email).Email();
        RuleFor(x => x.Password).StrongPassword();
        RuleFor(x => x.Subject)
            .NotEmpty()
            .WithMessage("Subject Can't Be Empty")
            .WithErrorCode(DomainErrors.TeacherErrors.MustHaveSubject);
        RuleForEach(x => x.AcademicYears).NotEmpty();
    }
}
