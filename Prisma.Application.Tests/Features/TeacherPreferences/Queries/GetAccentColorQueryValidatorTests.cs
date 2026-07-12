using FluentValidation.TestHelper;
using Prisma.Application.Features.TeacherPreferences.Queries.GetAccentColor;

namespace Prisma.Application.Tests.Features.TeacherPreferences.Queries;

public class GetAccentColorQueryValidatorTests
{
    private readonly GetAccentColorQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenEmailIsEmpty_HasValidationError()
    {
        var result = _validator.TestValidate(new GetAccentColorQuery(""));
        result.ShouldHaveValidationErrorFor(x => x.TeacherEmail);
    }

    [Fact]
    public void Validate_WhenEmailFormatIsInvalid_HasValidationError()
    {
        var result = _validator.TestValidate(new GetAccentColorQuery("not-an-email"));
        result.ShouldHaveValidationErrorFor(x => x.TeacherEmail);
    }

    [Fact]
    public void Validate_WhenEmailIsValid_HasNoValidationError()
    {
        var result = _validator.TestValidate(new GetAccentColorQuery("teacher@example.com"));
        result.ShouldNotHaveValidationErrorFor(x => x.TeacherEmail);
    }
}
