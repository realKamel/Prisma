using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses;
using Prisma.Application.Features.Authentication.Commands.EmailVerification;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;

namespace Prisma.Application.Tests.Features.Authentication.Commands;

public class EmailVerificationTests
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;
    private readonly EmailVerificationRequestHandler _requestHandler;
    private readonly ConfirmEmailHandler _confirmHandler;

    public EmailVerificationTests()
    {
        // Setup UserManager mock using the standard Identity test helper pattern
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(store, null!, null!, null!, null!, null!, null!, null!, null!);

        _emailService = Substitute.For<IEmailService>();
        _config = Substitute.For<IConfiguration>();

        // Mock AppSettings config mapping
        _config["AppSettings:BaseUrl"].Returns("https://testapi.prisma.com");

        _requestHandler = new EmailVerificationRequestHandler(_userManager, _emailService);
        _confirmHandler = new ConfirmEmailHandler(_userManager);
    }

    #region EmailVerificationRequestHandler Tests

    [Fact]
    public async Task Request_WhenUserDoesNotExist_ThrowsBadRequestException()
    {
        // Arrange
        var command = new EmailVerificationRequestCommand("ghost@prisma.com");
        _userManager.FindByEmailAsync(command.Email).Returns((User?)null);

        // Act
        var act = async () => await _requestHandler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Something Went Wrong");
        await _emailService.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default!);
    }

    [Fact]
    public async Task Request_WhenEmailIsAlreadyConfirmed_ThrowsBadRequestException()
    {
        // Arrange
        var command = new EmailVerificationRequestCommand("verified@prisma.com");
        var user = new User { Email = command.Email, EmailConfirmed = true };
        _userManager.FindByEmailAsync(command.Email).Returns(user);

        // Act
        var act = async () => await _requestHandler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Something Went Wrong");
        await _emailService.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default!);
    }

    [Fact]
    public async Task Request_HappyPath_GeneratesTokenAndSendsEmail()
    {
        // Arrange
        var command = new EmailVerificationRequestCommand("newuser@prisma.com");
        var user = new User { Email = command.Email, EmailConfirmed = false };
        var generatedToken = "raw-token-123+xyz";
        var expectedEscapedToken = Uri.EscapeDataString(generatedToken);

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _userManager.GenerateEmailConfirmationTokenAsync(user).Returns(generatedToken);

        // Act
        var result = await _requestHandler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Message.Should().Be("Verification email sent successfully.");

        await _emailService.Received(1).SendAsync(
            command.Email,
            "Email Verification",
            Arg.Is<string>(body => body.Contains($"confirm-email?email={command.Email}&token={expectedEscapedToken}"))
        );
    }

    #endregion

    #region ConfirmEmailHandler Tests

    [Fact]
    public async Task Confirm_WhenUserDoesNotExist_ThrowsBadRequestException()
    {
        // Arrange
        var command = new ConfirmEmailCommand("ghost@prisma.com", "any-token");
        _userManager.FindByEmailAsync(command.Email).Returns((User?)null);

        // Act
        var act = async () => await _confirmHandler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Something Went Wrong");
    }

    [Fact]
    public async Task Confirm_WhenEmailIsAlreadyConfirmed_ThrowsBadRequestException()
    {
        // Arrange
        var command = new ConfirmEmailCommand("alreadyconfirmed@prisma.com", "any-token");
        var user = new User { Email = command.Email, EmailConfirmed = true };
        _userManager.FindByEmailAsync(command.Email).Returns(user);

        // Act
        var act = async () => await _confirmHandler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Something Went Wrong");
    }

    [Fact]
    public async Task Confirm_WhenTokenIsInvalidOrExpired_ThrowsBadRequestException()
    {
        // Arrange
        var command = new ConfirmEmailCommand("user@prisma.com", "invalid-token");
        var user = new User { Email = command.Email, EmailConfirmed = false };

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _userManager.ConfirmEmailAsync(user, command.Token)
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Invalid token." }));

        // Act
        var act = async () => await _confirmHandler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Invalid token.");
    }

    [Fact]
    public async Task Confirm_HappyPath_SuccessfullyConfirmsEmail()
    {
        // Arrange
        var command = new ConfirmEmailCommand("user@prisma.com", "valid-token");
        var user = new User { Email = command.Email, EmailConfirmed = false };

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _userManager.ConfirmEmailAsync(user, command.Token).Returns(IdentityResult.Success);

        // Act
        var result = await _confirmHandler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Message.Should().Be("Email Verified successfully.");
    }

    #endregion
}