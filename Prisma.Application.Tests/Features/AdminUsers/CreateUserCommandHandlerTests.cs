using Ardalis.Result;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Users.Commands.CreateUser;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Tests.Features.AdminUsers;

public class CreateUserCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly CreateUserCommandHandler _sut;

    public CreateUserCommandHandlerTests()
    {
        _sut = new CreateUserCommandHandler(_identityService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenEmailOrPhoneAlreadyExists_ThrowsConflictException()
    {
        // Arrange
        _identityService.FindByEmailOrPhoneAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Student());

        var command = new CreateUserCommand(
            "محمد", "علي", "حسن", "إبراهيم", "01012345678", "m@test.com", "Passw0rd!",
            AppRoles.Student, 1, Guid.NewGuid(), "01198765432", "English");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Conflict);
    }

    [Fact]
    public async Task Handle_WhenCreatingStudent_SetsStudentSpecificFieldsAndAssignsRole()
    {
        // Arrange
        _identityService.FindByEmailOrPhoneAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _identityService.CreateAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        _identityService.AddToRoleAsync(Arg.Any<User>(), AppRoles.Student).Returns(IdentityResult.Success);

        var teacherId = Guid.NewGuid();
        var command = new CreateUserCommand(
            "محمد", "علي", "حسن", "إبراهيم", "01012345678", "m@test.com", "Passw0rd!",
            AppRoles.Student, 2, teacherId, "01198765432", "English");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be(AppRoles.Student);
        result.Value.GradeId.Should().Be(2);
        result.Value.TeacherIds.Should().Contain(teacherId.ToString());

        await _identityService.Received(1).CreateAsync(
            Arg.Is<User>(u => u is Student && u.Email == "m@test.com"),
            "Passw0rd!");
        await _identityService.Received(1).AddToRoleAsync(Arg.Any<User>(), AppRoles.Student);
    }

    [Fact]
    public async Task Handle_WhenCreatingTeacher_DoesNotSetStudentOnlyFields()
    {
        // Arrange
        _identityService.FindByEmailOrPhoneAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _identityService.CreateAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        _identityService.AddToRoleAsync(Arg.Any<User>(), AppRoles.Teacher).Returns(IdentityResult.Success);

        var command = new CreateUserCommand(
            "سارة", "خالد", "عبدالله", "محمود", "01198765432", "s@test.com", "Passw0rd!",
            AppRoles.Teacher, null, null, null, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Value.Role.Should().Be(AppRoles.Teacher);
        result.Value.GradeId.Should().BeNull();
        result.Value.TeacherIds.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenIdentityCreateFails_ThrowsBadRequestException()
    {
        // Arrange
        _identityService.FindByEmailOrPhoneAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _identityService.CreateAsync(Arg.Any<User>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Weak password" }));

        var command = new CreateUserCommand(
            "سارة", "خالد", "عبدالله", "محمود", "01198765432", "s@test.com", "weak",
            AppRoles.Teacher, null, null, null, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public async Task Handle_WhenUnknownRole_ThrowsBadRequestException()
    {
        // Arrange
        _identityService.FindByEmailOrPhoneAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new CreateUserCommand(
            "س", "خ", "ع", "م", "01198765432", "s@test.com", "Passw0rd!",
            "SuperAdmin", null, null, null, null); // not a real role

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
    }
}