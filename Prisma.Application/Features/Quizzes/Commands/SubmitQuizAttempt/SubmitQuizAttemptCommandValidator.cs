using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using Prisma.Application.Features.Quizzes.Commands.SaveQuizAnswer;

namespace Prisma.Application.Features.Quizzes.Commands.SubmitQuizAttempt;

public sealed class SubmitQuizAttemptCommandValidator : AbstractValidator<SubmitQuizAttemptCommand>
{
    public SubmitQuizAttemptCommandValidator()
    {
        RuleFor(x => x.AttemptId).GreaterThan(0);

        RuleFor(x => x.Answers!)
            .Must(a => a.Count <= 200)
            .WithMessage("عدد الإجابات أكبر من المسموح")
            .When(x => x.Answers is not null);

        RuleForEach(x => x.Answers!).ChildRules(a =>
        {
            a.RuleFor(i => i.QuestionId).GreaterThan(0);
            a.RuleFor(i => i.TextAnswer).MaximumLength(SaveQuizAnswerCommandValidator.MaxTextLength);
        }).When(x => x.Answers is not null);
    }
}
