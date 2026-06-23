
using FluentValidation;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Commands.CreateQuiz;

public class CreateQuizQuestionDtoValidator: AbstractValidator<CreateQuizQuestionDto>
{
    public CreateQuizQuestionDtoValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("نص السؤال مطلوب")
            .MaximumLength(1000);

        RuleFor(x => x.Degree)
            .GreaterThan(0)
            .WithMessage("درجة السؤال لازم تكون أكبر من صفر");

        // MCQ: لازم خيارين على الأقل، وخيار صح واحد بالظبط
        RuleFor(x => x.Choices)
            .NotNull()
            .WithMessage("لازم تضيفي اختيارات للسؤال")

            .Must(c => c!.Count >= 2)
            .WithMessage("لازم خيارين على الأقل")

            .Must(c => c!.Count(ch => ch.IsCorrect) == 1)
            .WithMessage("لازم تحددي إجابة صحيحة واحدة بالظبط")

            .When(x => x.Type == QuestionType.MCQ);

        // TrueFalse: لازم خيارين بالظبط
        RuleFor(x => x.Choices)
            .NotNull()
            .Must(c => c!.Count == 2)
            .WithMessage("سؤال صح/غلط لازم يكون عنده خيارين بالظبط")

            .Must(c => c!.Count(ch => ch.IsCorrect) == 1)
            .WithMessage("لازم تحددي إجابة صحيحة واحدة بالظبط")

            .When(x => x.Type == QuestionType.TrueFalse);

        // Written: مفيش اختيارات خالص
        RuleFor(x => x.Choices)
            .Must(c => c == null || c.Count == 0)
            .WithMessage("سؤال الإجابة الكتابية ميكونش عنده اختيارات")

            .When(x => x.Type == QuestionType.Written);

        RuleForEach(x => x.Choices)
            .ChildRules(choice =>
            {
                choice.RuleFor(c => c.Text).NotEmpty().WithMessage("نص الاختيار مطلوب");
            })
            .When(x => x.Choices != null);
    }
}
