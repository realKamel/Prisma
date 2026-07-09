using FluentValidation.TestHelper;
using Prisma.Application.Features.Quizzes.Commands.CreateQuiz;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Enums;

namespace Prisma.Application.Tests.Features.Quizzes.Commands.NewFolder.CreateQuiz;



public class CreateQuizQuestionDtoValidatorTests
{
    private readonly CreateQuizQuestionDtoValidator _validator = new();

    private static CreateQuizChoiceDto Choice(string text, bool isCorrect) =>
        new() { Text = text, IsCorrect = isCorrect };

    #region Common fields

    [Fact]
    public void Validate_WhenTextIsEmpty_HasValidationError()
    {
        var dto = new CreateQuizQuestionDto { Text = "", Type = QuestionType.Written, Degree = 5m, ModelAnswer = "x" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Text);
    }

    [Fact]
    public void Validate_WhenTextExceedsMaxLength_HasValidationError()
    {
        var dto = new CreateQuizQuestionDto
        {
            Text = new string('a', 1001),
            Type = QuestionType.Written,
            Degree = 5m,
            ModelAnswer = "x"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Text);
    }

    [Fact]
    public void Validate_WhenDegreeIsZeroOrLess_HasValidationError()
    {
        var dto = new CreateQuizQuestionDto
        {
            Text = "Q",
            Type = QuestionType.Written,
            Degree = 0m,
            ModelAnswer = "x"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Degree);
    }

    #endregion

    #region MCQ

    [Fact]
    public void Validate_WhenMcqHasNullChoices_HasValidationError()
    {
        var dto = new CreateQuizQuestionDto { Text = "Q", Type = QuestionType.MCQ, Degree = 5m, Choices = null };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Choices);
    }

    [Fact]
    public void Validate_WhenMcqHasFewerThanTwoChoices_HasValidationError()
    {
        var dto = new CreateQuizQuestionDto
        {
            Text = "Q",
            Type = QuestionType.MCQ,
            Degree = 5m,
            Choices = new List<CreateQuizChoiceDto> { Choice("A", true) }
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Choices);
    }

    [Fact]
    public void Validate_WhenMcqHasNoCorrectChoice_HasValidationError()
    {
        var dto = new CreateQuizQuestionDto
        {
            Text = "Q",
            Type = QuestionType.MCQ,
            Degree = 5m,
            Choices = new List<CreateQuizChoiceDto> { Choice("A", false), Choice("B", false) }
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Choices);
    }

    [Fact]
    public void Validate_WhenMcqHasMoreThanOneCorrectChoice_HasValidationError()
    {
        var dto = new CreateQuizQuestionDto
        {
            Text = "Q",
            Type = QuestionType.MCQ,
            Degree = 5m,
            Choices = new List<CreateQuizChoiceDto> { Choice("A", true), Choice("B", true) }
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Choices);
    }

    [Fact]
    public void Validate_WhenMcqIsValid_HasNoValidationErrors()
    {
        var dto = new CreateQuizQuestionDto
        {
            Text = "Q",
            Type = QuestionType.MCQ,
            Degree = 5m,
            Choices = new List<CreateQuizChoiceDto> { Choice("A", true), Choice("B", false), Choice("C", false) }
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region TrueFalse

    [Fact]
    public void Validate_WhenTrueFalseHasNotExactlyTwoChoices_HasValidationError()
    {
        var dto = new CreateQuizQuestionDto
        {
            Text = "Q",
            Type = QuestionType.TrueFalse,
            Degree = 5m,
            Choices = new List<CreateQuizChoiceDto> { Choice("True", true), Choice("False", false), Choice("Maybe", false) }
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Choices);
    }

    [Fact]
    public void Validate_WhenTrueFalseHasNoCorrectChoice_HasValidationError()
    {
        var dto = new CreateQuizQuestionDto
        {
            Text = "Q",
            Type = QuestionType.TrueFalse,
            Degree = 5m,
            Choices = new List<CreateQuizChoiceDto> { Choice("True", false), Choice("False", false) }
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Choices);
    }

    [Fact]
    public void Validate_WhenTrueFalseIsValid_HasNoValidationErrors()
    {
        var dto = new CreateQuizQuestionDto
        {
            Text = "Q",
            Type = QuestionType.TrueFalse,
            Degree = 5m,
            Choices = new List<CreateQuizChoiceDto> { Choice("True", true), Choice("False", false) }
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Written

    [Fact]
    public void Validate_WhenWrittenHasChoices_HasValidationError()
    {
        var dto = new CreateQuizQuestionDto
        {
            Text = "Q",
            Type = QuestionType.Written,
            Degree = 5m,
            ModelAnswer = "x",
            Choices = new List<CreateQuizChoiceDto> { Choice("A", true) }
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Choices);
    }

    [Fact]
    public void Validate_WhenWrittenHasNullChoices_HasNoValidationErrors()
    {
        var dto = new CreateQuizQuestionDto
        {
            Text = "Q",
            Type = QuestionType.Written,
            Degree = 5m,
            ModelAnswer = "x",
            Choices = null
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenWrittenHasEmptyChoicesList_HasNoValidationErrors()
    {
        var dto = new CreateQuizQuestionDto
        {
            Text = "Q",
            Type = QuestionType.Written,
            Degree = 5m,
            ModelAnswer = "x",
            Choices = new List<CreateQuizChoiceDto>()
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Choice-level text validation

    [Fact]
    public void Validate_WhenAnyChoiceTextIsEmpty_HasValidationError()
    {
        var dto = new CreateQuizQuestionDto
        {
            Text = "Q",
            Type = QuestionType.MCQ,
            Degree = 5m,
            Choices = new List<CreateQuizChoiceDto> { Choice("", true), Choice("B", false) }
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor("Choices[0].Text");
    }

    #endregion
}
