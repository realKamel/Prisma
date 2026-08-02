using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Application.Features.Students.Commands.ChangePasswordCommand;
using Prisma.Domain.Entities.UserAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Prisma.Application.Tests.Features.Students.Commands;

public class ChangePasswordCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    private readonly UserManager<User> _userManager = Substitute.For<UserManager<User>>(
        Substitute.For<IUserStore<User>>(), null, null, null, null, null, null, null, null);

    private readonly ChangePasswordCommandHandler _sut;

    public ChangePasswordCommandHandlerTests()
    {
        _sut = new ChangePasswordCommandHandler(_userManager, _currentUserService);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsUnauthorizedException()
    {
        // Arrange
        var command = new ChangePasswordCommand("OldPass123!", "NewPass123!");
        _currentUserService.UserId.Returns((Guid?)null); // مستخدم غير مسجل دخول

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var command = new ChangePasswordCommand("OldPass123!", "NewPass123!");
        _currentUserService.UserId.Returns(currentUserId);

        // محاكاة عدم العثور على المستخدم
        _userManager.FindByIdAsync(currentUserId.ToString()).Returns((User)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenIdentityResultFails_ReturnsFailureResultWithErrorMessage()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var fakeUser = new User { Id = currentUserId };
        var command = new ChangePasswordCommand("WrongOldPass!", "NewPass123!");

        _currentUserService.UserId.Returns(currentUserId);
        _userManager.FindByIdAsync(currentUserId.ToString()).Returns(fakeUser);

        // تجهيز خطأ وهمي من الـ Identity (مثلاً كلمة المرور الحالية غير صحيحة)
        var identityErrors = new IdentityError[]
        {
            new() { Description = "Password mismatch." }
        };
        var identityResult = IdentityResult.Failed(identityErrors);

        _userManager.ChangePasswordAsync(fakeUser, command.CurrentPassword, command.NewPassword)
            .Returns(identityResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.GetResultMessage().Should().Contain("Password mismatch.");
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenPasswordChangedSuccessfully_ReturnsSuccessResult()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var fakeUser = new User { Id = currentUserId };
        var command = new ChangePasswordCommand("CorrectOldPass123!", "BrandNewPass123!");

        _currentUserService.UserId.Returns(currentUserId);
        _userManager.FindByIdAsync(currentUserId.ToString()).Returns(fakeUser);

        // محاكاة نجاح عملية تغيير كلمة المرور في الـ Identity
        _userManager.ChangePasswordAsync(fakeUser, command.CurrentPassword, command.NewPassword)
            .Returns(IdentityResult.Success);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        // التعديل الصحيح: هنا نتوقع أن تكون الرسالة "Success" بدلاً من null بناءً على منطق الـ Base Result لديك عند النجاح
        result.GetResultMessage().Should().Be("Success");
    }
}