using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace Prisma.Application.Features.Quizzes.Commands.SaveQuizAnswer;

public class SaveQuizAnswerCommandValidator:AbstractValidator<SaveQuizAnswerCommand>
{
    public SaveQuizAnswerCommandValidator()
    {
        RuleFor(x => x.AttemptId).GreaterThan(0);
        RuleFor(x => x.QuestionId).GreaterThan(0);
        RuleFor(x => x)
            .Must(x => x.ChoiceId.HasValue ^ !string.IsNullOrWhiteSpace(x.TextAnswer) || (x.ChoiceId is null && x.TextAnswer is null))
            .WithMessage("يجب تحديد إجابة واحدة فقط (اختيار أو نص)");
    }
}

