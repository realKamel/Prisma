using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Assistants.Commands.CreateAssistant;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;

namespace Prisma.Application.Tests.Features.Assistants.Commands;

public class CreateAssistantCommandHandlerTests
{
    private readonly IIdentityService _identityService;
    private readonly CreateAssistantCommandHandler _handler;

    public CreateAssistantCommandHandlerTests()
    {
        _identityService = Substitute.For<IIdentityService>();
        _handler = new CreateAssistantCommandHandler(_identityService);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_CreatesAssistantSuccessfully()
    {
        // Arrange
        var command = new CreateAssistantCommand(
            "John",
            "Doe",
            "john.doe@example.com",
            "+1234567890",
            "SecureP@ss123!",
            ["Policy1", "Policy2"]
        );

        _identityService
            .FindByPhoneNumberAsync(command.PhoneNumber, Arg.Any<CancellationToken>())
            .Returns((Assistant)null!);

        _identityService
            .CreateAsync(Arg.Any<Assistant>(), command.Password)
            .Returns(IdentityResult.Success);

        _identityService
            .AddClaimsAsync(Arg.Any<Assistant>(), Arg.Any<IEnumerable<Claim>>())
            .Returns(IdentityResult.Success);

        _identityService
            .AddToRoleAsync(Arg.Any<Assistant>(), AppRoles.Assistant)
            .Returns(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.FirstName.Should().Be(command.FirstName);
        // result.Data.SecondName.Should().Be(command.LastName);
        result.Data.Policies.Should().BeEquivalentTo(command.Policies);
        result.Data.Id.Should().NotBeEmpty();

        await _identityService
            .Received(1)
            .FindByPhoneNumberAsync(command.PhoneNumber, Arg.Any<CancellationToken>());

        await _identityService
            .Received(1)
            .CreateAsync(
                Arg.Is<Assistant>(a =>
                    a.FirstName == command.FirstName &&
                    a.LastName == command.LastName &&
                    a.UserName == command.Email &&
                    a.Email == command.Email &&
                    a.PhoneNumber == command.PhoneNumber),
                command.Password);

        await _identityService
            .Received(1)
            .AddClaimsAsync(
                Arg.Any<Assistant>(),
                Arg.Is<IEnumerable<Claim>>(claims =>
                    claims.All(c => c.Type == AppClaims.PermissionsClaim)));

        await _identityService
            .Received(1)
            .AddToRoleAsync(Arg.Any<Assistant>(), AppRoles.Assistant);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyExists_ThrowsConflictException()
    {
        // Arrange
        var command = new CreateAssistantCommand(
            "John",
            "Doe",
            "john.doe@example.com",
            "+1234567890",
            "SecureP@ss123!",
            ["Policy1", "Policy2"]
        );


        var existingUser = new Assistant { PhoneNumber = command.PhoneNumber };

        _identityService
            .FindByPhoneNumberAsync(command.PhoneNumber, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("User already exists");

        await _identityService
            .DidNotReceive()
            .CreateAsync(Arg.Any<Assistant>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenCreateAsyncFails_ThrowsBadRequestException()
    {
        // Arrange
        var command = new CreateAssistantCommand(
            "John",
            "Doe",
            "john.doe@example.com",
            "+1234567890",
            "SecureP@ss123!",
            ["Policy1", "Policy2"]
        );


        _identityService
            .FindByPhoneNumberAsync(command.PhoneNumber, Arg.Any<CancellationToken>())
            .Returns((Assistant)null!);

        var identityErrors = new[]
        {
            new IdentityError { Code = "DuplicateUserName", Description = "Username already taken" },
            new IdentityError { Code = "PasswordTooShort", Description = "Password is too short" }
        };

        _identityService
            .CreateAsync(Arg.Any<Assistant>(), command.Password)
            .Returns(IdentityResult.Failed(identityErrors));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<BadRequestException>();
        exception.Which.Message.Should().Contain("DuplicateUserName");
        exception.Which.Message.Should().Contain("PasswordTooShort");

        await _identityService
            .DidNotReceive()
            .AddClaimsAsync(Arg.Any<Assistant>(), Arg.Any<IEnumerable<Claim>>());
    }

    [Fact]
    public async Task Handle_WhenAddClaimsAsyncFails_ThrowsBadRequestException()
    {
        // Arrange
        var command = new CreateAssistantCommand(
            "John",
            "Doe",
            "john.doe@example.com",
            "+1234567890",
            "SecureP@ss123!",
            ["Policy1", "Policy2"]
        );


        _identityService
            .FindByPhoneNumberAsync(command.PhoneNumber, Arg.Any<CancellationToken>())
            .Returns((Assistant)null!);

        _identityService
            .CreateAsync(Arg.Any<Assistant>(), command.Password)
            .Returns(IdentityResult.Success);

        var claimErrors = new[] { new IdentityError { Code = "ClaimError", Description = "Invalid claim" } };

        _identityService
            .AddClaimsAsync(Arg.Any<Assistant>(), Arg.Any<IEnumerable<Claim>>())
            .Returns(IdentityResult.Failed(claimErrors));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<BadRequestException>();
        exception.Which.Message.Should().Contain("ClaimError");

        await _identityService
            .DidNotReceive()
            .AddToRoleAsync(Arg.Any<Assistant>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenAddToRoleAsyncFails_ThrowsBadRequestException()
    {
        // Arrange
        var command = new CreateAssistantCommand(
            "John",
            "Doe",
            "john.doe@example.com",
            "+1234567890",
            "SecureP@ss123!",
            ["Policy1", "Policy2"]
        );


        _identityService
            .FindByPhoneNumberAsync(command.PhoneNumber, Arg.Any<CancellationToken>())
            .Returns((Assistant)null!);

        _identityService
            .CreateAsync(Arg.Any<Assistant>(), command.Password)
            .Returns(IdentityResult.Success);

        _identityService
            .AddClaimsAsync(Arg.Any<Assistant>(), Arg.Any<IEnumerable<Claim>>())
            .Returns(IdentityResult.Success);

        var roleErrors = new[] { new IdentityError { Code = "RoleError", Description = "Role assignment failed" } };

        _identityService
            .AddToRoleAsync(Arg.Any<Assistant>(), AppRoles.Assistant)
            .Returns(IdentityResult.Failed(roleErrors));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<BadRequestException>();
        exception.Which.Message.Should().Contain("RoleError");
    }

    [Fact]
    public async Task Handle_FiltersOutInvalidPolicies_BeforeAddingClaims()
    {
        // Arrange
        var validPolicy = AppClaims.Policies.All.First();
        var invalidPolicy = "InvalidPolicy";

        var command = new CreateAssistantCommand(
            "John",
            "Doe",
            "john.doe@example.com",
            "+1234567890",
            "SecureP@ss123!",
            [validPolicy, invalidPolicy]
        );


        IEnumerable<Claim>? capturedClaims = null;

        _identityService
            .FindByPhoneNumberAsync(command.PhoneNumber, Arg.Any<CancellationToken>())
            .Returns((Assistant)null!);

        _identityService
            .CreateAsync(Arg.Any<Assistant>(), command.Password)
            .Returns(IdentityResult.Success);

        _identityService
            .AddClaimsAsync(Arg.Any<Assistant>(), Arg.Any<IEnumerable<Claim>>())
            .Returns(callInfo =>
            {
                capturedClaims = callInfo.ArgAt<IEnumerable<Claim>>(1);
                return IdentityResult.Success;
            });

        _identityService
            .AddToRoleAsync(Arg.Any<Assistant>(), AppRoles.Assistant)
            .Returns(IdentityResult.Success);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedClaims.Should().NotBeNull();
        capturedClaims.Should().HaveCount(1);
        capturedClaims!.First().Value.Should().Be(validPolicy);
    }
}