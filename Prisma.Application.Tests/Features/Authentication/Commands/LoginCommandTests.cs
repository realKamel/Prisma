using System.Security.Claims;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Authentication.Commands.Login;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;


namespace Prisma.Application.Tests.Features.Authentication.Commands;

public class LoginCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly LoginCommandHandler _sut;

    private readonly User _fakeUser = new()
    {
        Id = Guid.CreateVersion7(), Email = "user@test.com", FirstName = "John", LastName = "Doe",
    };

    public LoginCommandHandlerTests()
    {
        _sut = new LoginCommandHandler(_identityService, _jwtTokenService);
    }

    // ── Happy path ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var command = new LoginCommand("user@test.com", null, "P@ssw0rd");
        var roles = new List<string> { AppRoles.Student };
        var permissions = new List<Claim>();

        _identityService.FindByEmailOrPhoneAsync(command.Email, command.Email)
            .Returns(_fakeUser);
        _identityService.CheckPasswordAsync(_fakeUser, command.Password)
            .Returns(true);
        _identityService.GetRolesAsync(_fakeUser)
            .Returns(roles);
        _identityService.GetClaimsAsync(_fakeUser)
            .Returns(permissions);
        _jwtTokenService.GenerateAccessToken(default!, default!, default!, default!)
            .ReturnsForAnyArgs(AppCookies.AccessToken);
        _jwtTokenService.GenerateRefreshToken()
            .Returns(AppCookies.RefreshToken);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.AccessToken.Should().Be(AppCookies.AccessToken);
        result.Data.RefreshToken.Should().Be(AppCookies.RefreshToken);
        result.Data.Credentials.Id.Should().Be(_fakeUser.Id);
        result.Data.Credentials.Email.Should().Be(_fakeUser.Email);
        result.Data.Credentials.Role.Should().Be(AppRoles.Student);
    }

    [Fact]
    public async Task Handle_ValidCredentials_SetsRefreshTokenOnUser()
    {
        // Arrange
        var command = new LoginCommand("user@test.com", null, "P@ssw0rd");

        _identityService.FindByEmailOrPhoneAsync(command.Email, command.Email)
            .Returns(_fakeUser);
        _identityService.CheckPasswordAsync(_fakeUser, command.Password)
            .Returns(true);
        _identityService.GetRolesAsync(_fakeUser).Returns(new List<string>());
        _identityService.GetClaimsAsync(_fakeUser)
            .Returns(new List<Claim>());
        _jwtTokenService.GenerateAccessToken(default!, default!, default!, default!)
            .ReturnsForAnyArgs("access-token");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh-token");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _fakeUser.RefreshToken.Should().Be("refresh-token");
        _fakeUser.RefreshTokenExpiry.Should().BeCloseTo(
            DateTimeOffset.UtcNow.AddDays(7), precision: TimeSpan.FromSeconds(5));
        _fakeUser.IsOnline.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidCredentials_CallsUpdateAsync()
    {
        // Arrange
        var command = new LoginCommand("user@test.com", null, "P@ssw0rd");

        _identityService.FindByEmailOrPhoneAsync(command.Email, command.Email)
            .Returns(_fakeUser);
        _identityService.CheckPasswordAsync(_fakeUser, command.Password)
            .Returns(true);
        _identityService.GetRolesAsync(_fakeUser).Returns(new List<string>());
        _identityService.GetClaimsAsync(_fakeUser)
            .Returns(new List<System.Security.Claims.Claim>());
        _jwtTokenService.GenerateAccessToken(default!, default!, default!, default!)
            .ReturnsForAnyArgs("at");
        _jwtTokenService.GenerateRefreshToken().Returns("rt");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _identityService.Received(1).UpdateAsync(_fakeUser);
    }

    [Fact]
    public async Task Handle_NoRoles_ReturnsNullRoleInResponse()
    {
        // Arrange
        var command = new LoginCommand("user@test.com", null, "P@ssw0rd");

        _identityService.FindByEmailOrPhoneAsync(command.Email, command.Email)
            .Returns(_fakeUser);
        _identityService.CheckPasswordAsync(_fakeUser, command.Password)
            .Returns(true);
        _identityService.GetRolesAsync(_fakeUser).Returns(new List<string>()); // empty
        _identityService.GetClaimsAsync(_fakeUser)
            .Returns(new List<System.Security.Claims.Claim>());
        _jwtTokenService.GenerateAccessToken(default!, default!, default!, default!)
            .ReturnsForAnyArgs("at");
        _jwtTokenService.GenerateRefreshToken().Returns("rt");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Data.Credentials.Role.Should().BeNull();
    }

    // ── User not found ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_UserNotFound_ThrowsBadRequestException()
    {
        // Arrange
        var command = new LoginCommand("ghost@test.com", null, "P@ssw0rd");

        _identityService.FindByEmailOrPhoneAsync(command.Email, command.Email)
            .Returns((User?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Invalid credentials*");
    }

    [Fact]
    public async Task Handle_UserNotFound_NeverChecksPassword()
    {
        // Arrange
        var command = new LoginCommand("ghost@test.com", null, "P@ssw0rd");

        _identityService.FindByEmailOrPhoneAsync(command.Email, command.Email)
            .Returns((User?)null);

        // Act
        try { await _sut.Handle(command, CancellationToken.None); }
        catch { }

        // Assert
        await _identityService.DidNotReceive()
            .CheckPasswordAsync(Arg.Any<User>(), Arg.Any<string>());
    }

    // ── Wrong password ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WrongPassword_ThrowsBadRequestException()
    {
        // Arrange
        var command = new LoginCommand("user@test.com", null, "wrong");

        _identityService.FindByEmailOrPhoneAsync(command.Email, command.Email)
            .Returns(_fakeUser);
        _identityService.CheckPasswordAsync(_fakeUser, command.Password)
            .Returns(false);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Invalid credentials*");
    }

    [Fact]
    public async Task Handle_WrongPassword_NeverGeneratesTokens()
    {
        // Arrange
        var command = new LoginCommand("user@test.com", null, "wrong");

        _identityService.FindByEmailOrPhoneAsync(command.Email, command.Email)
            .Returns(_fakeUser);
        _identityService.CheckPasswordAsync(_fakeUser, command.Password)
            .Returns(false);

        // Act
        try { await _sut.Handle(command, CancellationToken.None); }
        catch { }

        // Assert
        _jwtTokenService.DidNotReceive()
            .GenerateAccessToken(Arg.Any<Guid>(), Arg.Any<string>(),
                Arg.Any<List<string>>(), Arg.Any<List<Claim>>());
        _jwtTokenService.DidNotReceive().GenerateRefreshToken();
    }
}