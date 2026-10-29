using FluentValidation;

namespace Prisma.Application.Features.Lessons.Commands.AddLesson;

public class AddLessonCommandValidator : AbstractValidator<AddLessonCommand>
{
    public AddLessonCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Price must be 0 or greater.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required.")
            .Length(3)
            .WithMessage("Currency must be a valid 3-letter ISO code (e.g., USD, EGP).");

        RuleFor(x => x.AcademicYearIds)
            .NotEmpty()
            .WithMessage("At least one academic year must be selected.");

        RuleFor(x => x.ValidityDurationInSeconds)
            .GreaterThan(0)
            .WithMessage("Validity duration must be greater than zero.");

        // Conditional validation: Check due date only if AssignmentEnabled is true
        When(
            x => x.AssignmentEnabled,
            () =>
            {
                RuleFor(x => x.AssignmentDueDate)
                    .NotNull()
                    .WithMessage("Assignment due date is required when assignments are enabled.")
                    .GreaterThan(DateTimeOffset.UtcNow)
                    .WithMessage("Assignment due date must be in the future.");
            }
        );
    }
}
