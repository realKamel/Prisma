using FluentValidation;

namespace Prisma.Application.Features.Quizzes.Commands.OverrideAttemptScore;

public class OverrideAttemptScoreCommandValidator : AbstractValidator<OverrideAttemptScoreCommand>
{
    public OverrideAttemptScoreCommandValidator()
    {
        RuleFor(x => x.AttemptId)
            .GreaterThan(0);

        RuleFor(x => x.PenaltyScore)
            .GreaterThanOrEqualTo(0).WithMessage("الخصم لازم يكون صفر أو أكبر");
    }
}