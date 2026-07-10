using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Users.Queries.GetUserById;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;

namespace Prisma.Application.Tests.Features.Users.Queries;

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
        result.Succeeded.Should().BeTrue();
        result.Data.Role.Should().Be("Student");
        result.Data.GradeId.Should().Be(2);
        result.Data.TeacherId.Should().Be(teacherId);
        result.Data.ParentMobile.Should().Be("01198765432");
    }

    [Fact]
    public async Task Handle_WhenAssistant_TeacherIdIsAlwaysNull()
    {
        // Arrange
        var assistant = new Assistant
        {
            Id = Guid.CreateVersion7(),
            FirstName = "فاطمة",
            LastName = "أحمد",
            Email = "f@test.com",
        };

        _identityService
            .FindByIdAsync(assistant.Id, Arg.Any<CancellationToken>())
            .Returns(assistant);

        // Act
        var result = await _sut.Handle(new GetUserByIdQuery(assistant.Id), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Role.Should().Be("Assistant");
        result.Data.TeacherId.Should().BeNull();
        result.Data.GradeId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _identityService
            .FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var act = () => _sut.Handle(new GetUserByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}