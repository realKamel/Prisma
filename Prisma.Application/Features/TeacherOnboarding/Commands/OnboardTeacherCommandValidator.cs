using FluentValidation;
using Prisma.Application.Common.Validators.ValidationExtensions;
using Prisma.Domain.Errors;

namespace Prisma.Application.Features.TeacherOnboarding.Commands;

public sealed class OnboardTeacherCommandValidator : AbstractValidator<OnboardTeacherCommand>
{
    public OnboardTeacherCommandValidator()
    {
        RuleFor(x => x.FirstName).MustBeValidPersonName();
        RuleFor(x => x.SecondName).MustBeValidPersonName();
        RuleFor(x => x.ThirdName).MustBeValidPersonName();
        RuleFor(x => x.LastName).MustBeValidPersonName();

        RuleFor(x => x.PhoneNumber).MustBeValidPhoneNumber();

        RuleFor(x => x.Email).MustBeValidEmail();

        RuleFor(x => x.Password).MustBeStrongPassword();

        RuleFor(x => x.Subject)
            .NotEmpty()
            .WithMessage("Subject Can't Be Empty")
            .WithErrorCode(DomainErrors.TeacherErrors.MustHaveSubject);

        RuleForEach(x => x.AcademicYears).NotEmpty();
    }
}
