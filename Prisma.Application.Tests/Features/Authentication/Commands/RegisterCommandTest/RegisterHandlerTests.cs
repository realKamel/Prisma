using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Authentication.Commands.Register;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Tests.Features.Authentication.Commands.RegisterCommandTest;

public class RegisterHandlerTests
{
    // private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    // private readonly RegisterCommandHandler _handler;
    //
    // public RegisterHandlerTests()
    // {
    //     _handler = new RegisterCommandHandler(_identityService);
    // }
    //
    // [Fact]
    // public async Task Handle_WithNewValidUser_CreatesUserAndReturnsId()
    // {
    //     // Arrange
    //     var command = new RegisterCommand(
    //         Email: "john.doe@example.com",
    //         Password: "SecurePass123!",
    //         ConfirmPassword: "SecurePass123!",
    //         FirstName: "John",
    //         LastName: "Doe",
    //         SecondName: null,
    //         ThirdName: null,
    //         PhoneNumber: "555-555-5555",
    //         AcademicYear: null,
    //         ParentPhoneNumber: null
    //     );
    //
    //     _identityService
    //         .FindByEmailAsync(command.Email)
    //         .Returns((User?)null);
    //
    //     _identityService
    //         .CreateAsync(Arg.Any<User>(), command.Password)
    //         .Returns(IdentityResult.Success);
    //
    //     _identityService
    //         .AddToRoleAsync(Arg.Any<User>(), AppClaims.Roles.Student)
    //         .Returns(IdentityResult.Success);
    //
    //     // Act
    //     var result = await _handler.Handle(command, CancellationToken.None);
    //
    //     // Assert
    //     result.Succeeded.Should().BeTrue();
    //     result..Should().NotBeEmpty();
    //
    //     // Verify interactions
    //     await _identityService.Received(1).FindByEmailAsync(command.Email);
    //
    //     await _identityService.Received(1).CreateAsync(
    //         Arg.Is<User>(u =>
    //             u.Email == command.Email &&
    //             u.FirstName == command.FirstName &&
    //             u.LastName == command.LastName),
    //         command.Password);
    //
    //     await _identityService.Received(1).AddToRoleAsync(
    //         Arg.Is<User>(u => u.Email == command.Email),
    //         "Student");
    // }
}