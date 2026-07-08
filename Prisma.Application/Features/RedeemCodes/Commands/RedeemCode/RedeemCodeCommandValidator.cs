using FluentValidation;

namespace Prisma.Application.Features.RedeemCodes.Commands.RedeemCode;

public class RedeemCodeCommandValidator : AbstractValidator<RedeemCodeCommand>
{
    public RedeemCodeCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MinimumLength(4)
            .MaximumLength(20);

        RuleFor(x => x.LessonId)
            .GreaterThan(0);
    }
}