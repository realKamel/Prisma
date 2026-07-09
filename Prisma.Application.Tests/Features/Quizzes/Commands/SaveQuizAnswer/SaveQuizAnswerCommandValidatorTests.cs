using FluentValidation.TestHelper;
using Prisma.Application.Features.Quizzes.Commands.SaveQuizAnswer;

namespace Prisma.Application.Tests.Features.Quizzes.Commands.SaveQuizAnswer;


public class SaveQuizAnswerCommandValidatorTests
{
    private readonly SaveQuizAnswerCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenAttemptIdIsZeroOrLess_HasValidationError()
    {
        var command = new SaveQuizAnswerCommand(AttemptId: 0, QuestionId: 1, ChoiceId: 5, TextAnswer: null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AttemptId);
    }

    [Fact]
    public void Validate_WhenQuestionIdIsZeroOrLess_HasValidationError()
    {
        var command = new SaveQuizAnswerCommand(AttemptId: 1, QuestionId: 0, ChoiceId: 5, TextAnswer: null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.QuestionId);
    }

    [Fact]
    public void Validate_WhenBothChoiceIdAndTextAnswerAreProvided_HasValidationError()
    {
        var command = new SaveQuizAnswerCommand(AttemptId: 1, QuestionId: 1, ChoiceId: 5, TextAnswer: "some text");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_WhenOnlyChoiceIdIsProvided_HasNoValidationErrors()
    {
        var command = new SaveQuizAnswerCommand(AttemptId: 1, QuestionId: 1, ChoiceId: 5, TextAnswer: null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenOnlyTextAnswerIsProvided_HasNoValidationErrors()
    {
        var command = new SaveQuizAnswerCommand(AttemptId: 1, QuestionId: 1, ChoiceId: null, TextAnswer: "my answer");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenNeitherChoiceIdNorTextAnswerProvided_HasNoValidationErrors()
    {
        // Note: this is allowed by the validator as-is — an empty/clearing answer.
        // Flagging this as worth confirming with the team: is clearing an answer a valid use case?
        var command = new SaveQuizAnswerCommand(AttemptId: 1, QuestionId: 1, ChoiceId: null, TextAnswer: null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenTextAnswerIsWhitespaceOnly_TreatedAsEmptyAndAllowsChoiceId()
    {
        // IsNullOrWhiteSpace treats "   " as empty, so this combination passes the "only one" rule
        var command = new SaveQuizAnswerCommand(AttemptId: 1, QuestionId: 1, ChoiceId: 5, TextAnswer: "   ");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
