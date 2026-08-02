using Ardalis.Result;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Users.Commands.DeleteUser;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Tests.Features.AdminUsers;

public class DeleteUserCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<User, Guid> _userRepo = Substitute.For<IRepository<User, Guid>>();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly DeleteUserCommandHandler _sut;

    public DeleteUserCommandHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<User, Guid>().Returns(_userRepo);
        _sut = new DeleteUserCommandHandler(_identityService);
    }

    [Fact]
    public async Task Handle_WhenUserExists_DeletesAndReturnsSuccess()
    {
        // Arrange
        var user = new Student { Id = Guid.NewGuid() };
        _identityService.FindByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _identityService.DeleteAsync(user).Returns(IdentityResult.Success);

        // Act
        var result = await _sut.Handle(new DeleteUserCommand(user.Id), CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.NoContent);
        await _identityService.Received(1).DeleteAsync(user);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _identityService.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _sut.Handle(new DeleteUserCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        await _identityService.DidNotReceive().DeleteAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_WhenIdentityDeleteFails_ThrowsBadRequestException()
    {
        // Arrange
        var user = new Domain.Entities.UserAggregate.Teacher { Id = Guid.NewGuid() };
        _identityService.FindByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _identityService.DeleteAsync(user)
            .Returns(IdentityResult.Failed(new IdentityError { Description = "DB constraint" }));

        // Act
        var result = await _sut.Handle(new DeleteUserCommand(user.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
    }
}