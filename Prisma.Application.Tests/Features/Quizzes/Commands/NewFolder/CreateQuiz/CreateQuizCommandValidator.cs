
using FluentValidation.TestHelper;
using Prisma.Application.Features.Quizzes.Commands.CreateQuiz;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Enums;

namespace Prisma.Application.Tests.Features.Quizzes.Commands.NewFolder.CreateQuiz;

public class CreateQuizCommandValidatorTests
{
    private readonly CreateQuizCommandValidator _validator = new();

    private static CreateQuizChoiceDto Choice(string text, bool isCorrect) =>
        new() { Text = text, IsCorrect = isCorrect };

    private static CreateQuizQuestionDto ValidMcqQuestion() =>
        new()
        {
            Text = "What is 2+2?",
            Type = QuestionType.MCQ,
            Degree = 5m,
            Choices = new List<CreateQuizChoiceDto> { Choice("3", false), Choice("4", true) }
        };

    private static CreateQuizCommand ValidLessonQuizCommand() =>
        new(
            Title: "Quiz 1",
            Description: "desc",
            Scope: QuizScope.LessonQuiz,
            LessonId: 10,
            AcademicYearId: null,
            DurationMinutes: 30,
            AvailableFrom: null,
            DueDate: null,
            Questions: new List<CreateQuizQuestionDto> { ValidMcqQuestion() });

    [Fact]
    public void Validate_WhenTitleIsEmpty_HasValidationError()
    {
        var command = ValidLessonQuizCommand() with { Title = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WhenTitleExceedsMaxLength_HasValidationError()
    {
        var command = ValidLessonQuizCommand() with { Title = new string('a', 251) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WhenDescriptionExceedsMaxLength_HasValidationError()
    {
        var command = ValidLessonQuizCommand() with { Description = new string('a', 1001) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WhenScopeIsLessonQuizAndLessonIdIsNull_HasValidationError()
    {
        var command = ValidLessonQuizCommand() with { LessonId = null };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LessonId);
    }

    [Fact]
    public void Validate_WhenScopeIsComprehensiveExamAndLessonIdIsProvided_HasValidationError()
    {
        var command = ValidLessonQuizCommand() with
        {
            Scope = QuizScope.ComprehensiveExam,
            LessonId = 10,
            AcademicYearId = 5
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LessonId);
    }

    [Fact]
    public void Validate_WhenScopeIsComprehensiveExamAndAcademicYearIdIsNull_HasValidationError()
    {
        var command = ValidLessonQuizCommand() with
        {
            Scope = QuizScope.ComprehensiveExam,
            LessonId = null,
            AcademicYearId = null
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AcademicYearId);
    }

    [Fact]
    public void Validate_WhenScopeIsComprehensiveExamWithValidAcademicYearId_HasNoValidationErrorsForScopeFields()
    {
        var command = ValidLessonQuizCommand() with
        {
            Scope = QuizScope.ComprehensiveExam,
            LessonId = null,
            AcademicYearId = 5
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.LessonId);
        result.ShouldNotHaveValidationErrorFor(x => x.AcademicYearId);
    }

    [Fact]
    public void Validate_WhenAvailableFromIsAfterDueDate_HasValidationErrorOnDueDate()
    {
        var command = ValidLessonQuizCommand() with
        {
            AvailableFrom = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero),
            DueDate = new DateTimeOffset(2026, 1, 5, 0, 0, 0, TimeSpan.Zero)
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("DueDate");
    }

    [Fact]
    public void Validate_WhenAvailableFromEqualsDueDate_HasValidationErrorOnDueDate()
    {
        var sameInstant = DateTimeOffset.UtcNow;
        var command = ValidLessonQuizCommand() with
        {
            AvailableFrom = sameInstant,
            DueDate = sameInstant
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("DueDate");
    }

    [Fact]
    public void Validate_WhenOnlyAvailableFromIsSet_HasNoDateValidationError()
    {
        var command = ValidLessonQuizCommand() with
        {
            AvailableFrom = DateTimeOffset.UtcNow,
            DueDate = null
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor("DueDate");
    }

    [Fact]
    public void Validate_WhenDatesAreNull_HasNoDateValidationError()
    {
        var command = ValidLessonQuizCommand() with { AvailableFrom = null, DueDate = null };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor("DueDate");
    }

    [Fact]
    public void Validate_WhenQuestionsListIsEmpty_HasValidationError()
    {
        var command = ValidLessonQuizCommand() with { Questions = new List<CreateQuizQuestionDto>() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Questions);
    }

    [Fact]
    public void Validate_WhenAllFieldsValid_HasNoValidationErrors()
    {
        var command = ValidLessonQuizCommand();
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
