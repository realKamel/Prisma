using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Users.Commands.UpdateUser;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Users;

namespace Prisma.Application.Tests.Features.Users.Commands;

public class UpdateUserCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<User, Guid> _userRepo = Substitute.For<IRepository<User, Guid>>();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly UserManager<User> _userManager = CreateUserManager();
    private readonly UpdateUserCommandHandler _sut;

    public UpdateUserCommandHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<User, Guid>().Returns(_userRepo);
        _sut = new UpdateUserCommandHandler( _identityService, _userManager);
    }

    // UserManager<T> is a concrete class with a large constructor, but its
    // members are virtual, so NSubstitute can still substitute it — this is
    // the standard pattern for testing ASP.NET Identity-dependent code.
    private static UserManager<User> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<User>>();
        return Substitute.For<UserManager<User>>(store, null, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsNotFoundException()
    {
        // Arrange — regression test for the FindByIdAsync bug, same root
        // cause as DeleteUserCommandHandlerTests.
        _userRepo.FirstOrDefaultAsync(Arg.Any<UserByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new UpdateUserCommand(
            Guid.NewGuid(), "أ", "ب", "ج", "د", "01012345678", "x@test.com", null, null, null, null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenEmailChangedToOneAlreadyUsedByAnotherUser_ThrowsConflictException()
    {
        // Arrange
        var student = new Student { Id = Guid.NewGuid(), Email = "old@test.com" };
        var otherUser = new Student { Id = Guid.NewGuid(), Email = "new@test.com" };

        _userRepo.FirstOrDefaultAsync(Arg.Any<UserByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(student);
        _identityService.FindByEmailAsync("new@test.com").Returns(otherUser);

        var command = new UpdateUserCommand(
            student.Id, "أ", "ب", "ج", "د", "01012345678", "new@test.com", null, 1, null, "01198765432");

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenEmailUnchanged_DoesNotCheckForDuplicateEmail()
    {
        // Arrange
        var student = new Student { Id = Guid.NewGuid(), Email = "same@test.com" };
        _userRepo.FirstOrDefaultAsync(Arg.Any<UserByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(student);
        _identityService.UpdateAsync(Arg.Any<User>()).Returns(IdentityResult.Success);

        var command = new UpdateUserCommand(
            student.Id, "أ", "ب", "ج", "د", "01012345678", "same@test.com", null, 1, null, "01198765432");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        await _identityService.DidNotReceive().FindByEmailAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenUpdatingStudent_UpdatesStudentSpecificFields()
    {
        // Arrange
        var student = new Student { Id = Guid.NewGuid(), Email = "s@test.com", TeacherId = Guid.NewGuid() };
        var newTeacherId = Guid.NewGuid();

        _userRepo.FirstOrDefaultAsync(Arg.Any<UserByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(student);
        _identityService.UpdateAsync(Arg.Any<User>()).Returns(IdentityResult.Success);

        var command = new UpdateUserCommand(
            student.Id, "محمد", "علي", "حسن", "إبراهيم", "01099999999", "s@test.com",
            null, 3, newTeacherId, "01188888888");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.GradeId.Should().Be(3);
        result.Data.TeacherId.Should().Be(newTeacherId);
        result.Data.ParentMobile.Should().Be("01188888888");
        student.FirstName.Should().Be("محمد");
    }

    [Fact]
    public async Task Handle_WhenIdentityUpdateFails_ThrowsBadRequestException()
    {
        // Arrange
        var student = new Student { Id = Guid.NewGuid(), Email = "s@test.com" };
        _userRepo.FirstOrDefaultAsync(Arg.Any<UserByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(student);
        _identityService.UpdateAsync(Arg.Any<User>())
            .Returns(IdentityResult.Failed(new IdentityError { Description = "concurrency error" }));

        var command = new UpdateUserCommand(
            student.Id, "أ", "ب", "ج", "د", "01012345678", "s@test.com", null, null, null, null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenNewPasswordProvided_RemovesOldPasswordAndAddsNewOne()
    {
        // Arrange
        var student = new Student { Id = Guid.NewGuid(), Email = "s@test.com" };
        _userRepo.FirstOrDefaultAsync(Arg.Any<UserByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(student);
        _identityService.UpdateAsync(Arg.Any<User>()).Returns(IdentityResult.Success);
        _userManager.RemovePasswordAsync(student).Returns(IdentityResult.Success);
        _userManager.AddPasswordAsync(student, "NewPassw0rd!").Returns(IdentityResult.Success);

        var command = new UpdateUserCommand(
            student.Id, "أ", "ب", "ج", "د", "01012345678", "s@test.com", "NewPassw0rd!", null, null, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        await _userManager.Received(1).RemovePasswordAsync(student);
        await _userManager.Received(1).AddPasswordAsync(student, "NewPassw0rd!");
    }
}