using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace Prisma.Application.Features.Quizzes.Commands.GradeWrittenAnswers;

public class GradeWrittenAnswersCommandValidator : AbstractValidator<GradeWrittenAnswersCommand>
{
    public GradeWrittenAnswersCommandValidator()
    {
        RuleFor(x => x.AttemptId).GreaterThan(0);

        RuleFor(x => x.Grades)
            .NotEmpty().WithMessage("لازم تحددي درجة لسؤال واحد على الأقل");

        RuleForEach(x => x.Grades).ChildRules(grade =>
        {
            grade.RuleFor(g => g.AnswerId)
                .GreaterThan(0);

            grade.RuleFor(g => g.Score)
                .GreaterThanOrEqualTo(0).WithMessage("الدرجة لازم تكون صفر أو أكبر");
        });
    }

}
