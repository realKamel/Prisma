using FluentValidation;

namespace Prisma.Application.Features.RedeemCodes.Commands.CreateCodeBatch;

public class CreateCodeBatchCommandValidator : AbstractValidator<CreateCodeBatchCommand>
{
    public CreateCodeBatchCommandValidator()
    {
        RuleFor(x => x.AcademicYearId)
            .GreaterThan(0);

        RuleFor(x => x.LessonId)
            .GreaterThan(0);

        RuleFor(x => x.Count)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Prefix)
            .Matches("^[A-Za-z]{3,6}$")
            .When(x => !string.IsNullOrWhiteSpace(x.Prefix))
            .WithMessage("Prefix must be 3-6 English letters.");
    }
}