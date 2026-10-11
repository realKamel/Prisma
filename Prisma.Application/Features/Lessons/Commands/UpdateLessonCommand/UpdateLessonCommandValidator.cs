using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace Prisma.Application.Features.Lessons.Commands.UpdateLessonCommand;

public class UpdateLessonCommandValidator : AbstractValidator<UpdateLessonDetailsCommand>
{
    public UpdateLessonCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("{PropertyName}  is required.");

        RuleFor(x => x.Title)
            .MinimumLength(5)
            .WithMessage("{PropertyName} must be at least 5 characters long.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("{PropertyName} must be greater than or equal to 0.");

        RuleFor(x => x.Chapters).NotEmpty().WithMessage("At least one chapter is required.");

        RuleFor(x => x.AcademicYearIds)
            .NotEmpty()
            .WithMessage("At least one academic year ID is required.");

        RuleFor(x => x.Outcomes).NotEmpty().WithMessage("At least one outcome is required.");

        RuleFor(x => x.ValidityUnTill)
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("{PropertyName} must be in the future");
    }
}
