using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Users.Queries.GetUserById;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Tests.Features.AdminUsers;

public class GetUserByIdQueryHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly GetUserByIdQueryHandler _sut;

    public GetUserByIdQueryHandlerTests()
    {
        _sut = new GetUserByIdQueryHandler(_identityService);
    }

    [Fact]
    public async Task Handle_WhenStudentExists_ReturnsStudentSpecificFields()
    {
        // Arrange
        var teacherId = Guid.CreateVersion7();
        var student = new Student
        {
            Id = Guid.CreateVersion7(),
            FirstName = "محمد",
            SecondName = "إبراهيم",
            ThirdName = "حسن",
            LastName = "علي",
            Email = "m@test.com",
            PhoneNumber = "01012345678",
            AcademicYearId = 2,
            TeacherId = teacherId,
            ParentPhoneNumber = "01198765432",
        };

        _identityService
            .FindByIdAsync(student.Id, Arg.Any<CancellationToken>())
            .Returns(student);

        // Act
        var result = await _sut.Handle(new GetUserByIdQuery(student.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be("Student");
        result.Value.GradeId.Should().Be(2);
        result.Value.TeacherId.Should().Be(teacherId);
        result.Value.ParentMobile.Should().Be("01198765432");
    }

    [Fact]
    public async Task Handle_WhenAssistant_TeacherIdIsAlwaysNull()
    {
        // Arrange
        var assistant = new Assistant
        {
            Id = Guid.CreateVersion7(), FirstName = "فاطمة", LastName = "أحمد", Email = "f@test.com",
        };

        _identityService
            .FindByIdAsync(assistant.Id, Arg.Any<CancellationToken>())
            .Returns(assistant);

        // Act
        var result = await _sut.Handle(new GetUserByIdQuery(assistant.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be("Assistant");
        result.Value.TeacherId.Should().BeNull();
        result.Value.GradeId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _identityService
            .FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _sut.Handle(new GetUserByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}