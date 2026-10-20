using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace Prisma.Application.Features.Quizzes.Commands.SaveQuizAnswer;

public class SaveQuizAnswerCommandValidator:AbstractValidator<SaveQuizAnswerCommand>
{
    public const int MaxTextLength = 10_000;
    public SaveQuizAnswerCommandValidator()
    {
        RuleFor(x => x.AttemptId).GreaterThan(0).WithMessage("المحاولة غير صالحة");
        RuleFor(x => x.QuestionId).GreaterThan(0).WithMessage("السؤال غير صالح");
        RuleFor(x => x.ClientSeq).GreaterThan(0).WithMessage("طلب غير صالح");


        RuleFor(x => x.TextAnswer)
            .MaximumLength(MaxTextLength)
            .WithMessage($"الإجابة طويلة جداً (الحد الأقصى {MaxTextLength} حرف)");

        // An answer is either a choice or a text, never both
        RuleFor(x => x)
            .Must(x => x.ChoiceId is null || string.IsNullOrEmpty(x.TextAnswer))
            .WithMessage("لا يمكن إرسال اختيار ونص معاً");
    }
}

