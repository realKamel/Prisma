using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Prisma.Application.Features.Lessons.Commands.ToggleLessonStatusCommand;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Tests.Features.Lessons.Commands;

public class ToggleLessonStatusCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
    private readonly ToggleLessonStatusCommandHandler _sut;

    public ToggleLessonStatusCommandHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _sut = new ToggleLessonStatusCommandHandler(_unitOfWork, _currentUserService, _identityService);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthorized_ThrowsUnauthorizedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new ToggleLessonStatusCommand(1);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenLessonNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId).Returns(fakeUser);
        _identityService.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });

        _lessonRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Lesson?)null);

        var command = new ToggleLessonStatusCommand(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenLessonIsDrafted_ThrowsBadRequestException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId).Returns(fakeUser);
        _identityService.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });

        var lesson = new Lesson { Id = 1, Status = LessonStatus.Drafted };
        _lessonRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(lesson);

        var command = new ToggleLessonStatusCommand(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain("Cannot toggle status for a drafted lesson.");
    }

    [Fact]
    public async Task Handle_ValidRequest_TogglesStatusCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId).Returns(fakeUser);
        _identityService.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });

        var lesson = new Lesson { Id = 1, Status = LessonStatus.Hidden };
        _lessonRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(lesson);

        var command = new ToggleLessonStatusCommand(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        lesson.Status.Should().Be(LessonStatus.Active); // تحولت من Hidden إلى Active

        _lessonRepo.Received(1).Update(lesson);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}