
using FluentValidation;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Commands.CreateQuiz;

public class CreateQuizCommandValidator: AbstractValidator<CreateQuizCommand>
{
    public CreateQuizCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان الاختبار مطلوب")
            .MaximumLength(250);


        RuleFor(x => x.Description)
            .MaximumLength(1000);

        // ── Scope - dependent rules ──────────────────────────────
        RuleFor(x => x.LessonId)
            .NotNull().WithMessage("لازم تختاري الحصة")
            .When(x => x.Scope == QuizScope.LessonQuiz);

        RuleFor(x => x.LessonId)
            .Null().WithMessage("الامتحان الشامل ميتربطش بحصة")
            .When(x => x.Scope == QuizScope.ComprehensiveExam);

        RuleFor(x => x.AcademicYearId)
            .NotNull().WithMessage("لازم تحددي السنة الدراسية للامتحان الشامل")
            .When(x => x.Scope == QuizScope.ComprehensiveExam);

        // ── Dates ───────────────────────────────────────────────
        RuleFor(x => x)
            .Must(x => !x.AvailableFrom.HasValue || !x.DueDate.HasValue || x.AvailableFrom < x.DueDate)
            .WithMessage("موعد البداية لازم يكون قبل موعد النهاية")
            .OverridePropertyName("DueDate");

        // ── Questions ───────────────────────────────────────────
        RuleFor(x => x.Questions)
            .NotEmpty().WithMessage("لازم تضيفي سؤال واحد على الأقل");

        RuleForEach(x => x.Questions).SetValidator(new CreateQuizQuestionDtoValidator());

    }
}
