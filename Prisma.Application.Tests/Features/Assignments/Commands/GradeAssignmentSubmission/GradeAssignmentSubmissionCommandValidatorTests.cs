using FluentValidation.TestHelper;
using Prisma.Application.Features.Assignments.Commands.GradeAssignmentSubmission;

namespace Prisma.Application.Tests.Features.Assignments.Commands.GradeAssignmentSubmission;


public class GradeAssignmentSubmissionCommandValidatorTests
{
    private readonly GradeAssignmentSubmissionCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenSubmissionIdIsZero_HasValidationError()
    {
        var command = new GradeAssignmentSubmissionCommand(0, 50, "Note");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SubmissionId);
    }

    [Fact]
    public void Validate_WhenSubmissionIdIsNegative_HasValidationError()
    {
        var command = new GradeAssignmentSubmissionCommand(-1, 50, "Note");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SubmissionId);
    }

    [Fact]
    public void Validate_WhenSubmissionIdIsPositive_HasNoValidationError()
    {
        var command = new GradeAssignmentSubmissionCommand(1, 50, "Note");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.SubmissionId);
    }

    [Fact]
    public void Validate_WhenScoreIsNegative_HasValidationError()
    {
        var command = new GradeAssignmentSubmissionCommand(1, -1, "Note");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Score)
            .WithErrorMessage("الدرجة لازم تكون صفر أو أكبر");
    }

    [Fact]
    public void Validate_WhenScoreIsZero_HasNoValidationError()
    {
        var command = new GradeAssignmentSubmissionCommand(1, 0, "Note");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Score);
    }

    [Fact]
    public void Validate_WhenScoreIsPositive_HasNoValidationError()
    {
        var command = new GradeAssignmentSubmissionCommand(1, 90, "Note");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Score);
    }

    [Fact]
    public void Validate_WhenNoteIsNull_HasNoValidationError()
    {
        var command = new GradeAssignmentSubmissionCommand(1, 90, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Note);
    }

    [Fact]
    public void Validate_WhenNoteIsWithinMaxLength_HasNoValidationError()
    {
        var command = new GradeAssignmentSubmissionCommand(1, 90, new string('a', 1000));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Note);
    }

    [Fact]
    public void Validate_WhenNoteExceedsMaxLength_HasValidationError()
    {
        var command = new GradeAssignmentSubmissionCommand(1, 90, new string('a', 1001));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Note);
    }
}
