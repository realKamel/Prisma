using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Authentication.Commands.Register;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;

namespace Prisma.Application.Tests.Features.Authentication.Commands;

public class RegisterCommandHandlerTests

{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly RegisterCommandHandler _handler;

    private static readonly RegisterCommand ValidCommand = new(
        Email: "john.doe@example.com",
        Password: "SecurePass123!",
        ConfirmPassword: "SecurePass123!",
        FirstName: "John",
        LastName: "Doe",
        SecondName: null,
        ThirdName: null,
        PhoneNumber: "01234567890",
        AcademicYear: null,
        ParentPhoneNumber: null
    );

    public RegisterCommandHandlerTests()
    {
        _handler = new RegisterCommandHandler(_identityService);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_CreatesStudentAndReturnsSuccess()
    {
        // Arrange
        _identityService
            .FindByEmailOrPhoneAsync(ValidCommand.Email, ValidCommand.PhoneNumber)
            .Returns((User?)null);

        _identityService
            .CreateAsync(Arg.Any<Student>(), ValidCommand.Password)
            .Returns(IdentityResult.Success);

        _identityService
            .AddToRoleAsync(Arg.Any<Student>(), AppRoles.Student)
            .Returns(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        await _identityService.Received(1).FindByEmailOrPhoneAsync(ValidCommand.Email, ValidCommand.PhoneNumber);

        await _identityService.Received(1).CreateAsync(
            Arg.Is<Student>(u =>
                u.Email == ValidCommand.Email &&
                u.UserName == ValidCommand.Email &&
                u.FirstName == ValidCommand.FirstName &&
                u.LastName == ValidCommand.LastName &&
                u.PhoneNumber == ValidCommand.PhoneNumber),
            ValidCommand.Password);

        await _identityService.Received(1).AddToRoleAsync(
            Arg.Is<Student>(u => u.Email == ValidCommand.Email),
            AppRoles.Student);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyExists_ThrowsBadRequestException()
    {
        // Arrange
        _identityService
            .FindByEmailOrPhoneAsync(ValidCommand.Email, ValidCommand.PhoneNumber)
            .Returns(new User());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(ValidCommand, CancellationToken.None));

        await _identityService.DidNotReceive().CreateAsync(Arg.Any<User>(), Arg.Any<string>());
        await _identityService.DidNotReceive().AddToRoleAsync(Arg.Any<User>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenCreateFails_ThrowsBadRequestExceptionAndSkipsRoleAssignment()
    {
        // Arrange
        var identityErrors = new[]
        {
            new IdentityError { Code = "PasswordTooWeak", Description = "Password is too weak." },
            new IdentityError { Code = "PasswordTooWeak", Description = "Password needs a symbol." },
            new IdentityError { Code = "DuplicateEmail", Description = "Email is already taken." }
        };

        _identityService
            .FindByEmailOrPhoneAsync(ValidCommand.Email, ValidCommand.PhoneNumber)
            .Returns((User?)null);

        _identityService
            .CreateAsync(Arg.Any<Student>(), ValidCommand.Password)
            .Returns(IdentityResult.Failed(identityErrors));

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(ValidCommand, CancellationToken.None));

        await _identityService.DidNotReceive().AddToRoleAsync(Arg.Any<User>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_MapsAllCommandFieldsToStudent()
    {
        // Arrange
        var command = ValidCommand with
        {
            SecondName = "Michael", ThirdName = "James", AcademicYear = 2, ParentPhoneNumber = "09876543210"
        };

        _identityService
            .FindByEmailOrPhoneAsync(command.Email, command.PhoneNumber)
            .Returns((User?)null);

        _identityService
            .CreateAsync(Arg.Any<Student>(), command.Password)
            .Returns(IdentityResult.Success);

        _identityService
            .AddToRoleAsync(Arg.Any<Student>(), AppRoles.Student)
            .Returns(IdentityResult.Success);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _identityService.Received(1).CreateAsync(
            Arg.Is<Student>(u =>
                u.Email == command.Email &&
                u.UserName == command.Email &&
                u.FirstName == command.FirstName &&
                u.SecondName == command.SecondName &&
                u.ThirdName == command.ThirdName &&
                u.LastName == command.LastName &&
                u.PhoneNumber == command.PhoneNumber &&
                u.AcademicYearId == command.AcademicYear &&
                u.ParentPhoneNumber == command.ParentPhoneNumber),
            command.Password);
    }
}