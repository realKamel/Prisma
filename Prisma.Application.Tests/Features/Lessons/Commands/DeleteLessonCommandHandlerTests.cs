using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Lessons.Commands.DeleteLessonCommand;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using static Prisma.Application.Features.Lessons.Commands.DeleteLessonCommand.DeleteLessonCommandHandler;

namespace Prisma.Application.Tests.Features.Lessons.Commands;

public class DeleteLessonCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
    private readonly IRepository<Enrollment, int> _enrollmentRepo = Substitute.For<IRepository<Enrollment, int>>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly IVideoStorageService _videoStorageService = Substitute.For<IVideoStorageService>();
    private readonly DeleteLessonCommandHandler _sut;

    public DeleteLessonCommandHandlerTests()
    {
        _storageService.DefaultBucketName.Returns("prisma");
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _unitOfWork.GetOrCreateRepository<Enrollment, int>().Returns(_enrollmentRepo);
        _sut = new DeleteLessonCommandHandler(
            _unitOfWork,
            _currentUserService,
            _identityService,
            _storageService,
            _videoStorageService);
    }

    // NOTE: assumes UserRole exposes a `Role` navigation property with a `Name` string,
    // matching handler's `r.Role.Name` usage. Adjust the constructor below if the real
    // UserRole/Role shape differs.
    private static User CreateUserWithRole(Guid userId, string roleName)
    {
        return new User
        {
            Id = userId,
            Roles = new List<UserRole>
            {
                new UserRole { Role = new Role { Name = roleName } }
            }
        };
    }

    private static LessonDeletionInfo CreateLessonInfo(
        int id = 1,
        string? imageThumbnailUrl = null,
        List<int>? enrollmentIds = null,
        List<string?>? sectionAssetIds = null)
    {
        return new LessonDeletionInfo(
            id,
            imageThumbnailUrl,
            enrollmentIds ?? new List<int>(),
            sectionAssetIds ?? new List<string?>());
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ReturnsUnauthorized()
    {
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new DeleteLessonCommand(1);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        await _identityService.DidNotReceive().FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsUnauthorized()
    {
        var userId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var command = new DeleteLessonCommand(1);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotHavePermission_ReturnsUnauthorized()
    {
        var userId = Guid.NewGuid();
        var fakeUser = CreateUserWithRole(userId, "Student");

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(fakeUser);

        var command = new DeleteLessonCommand(1);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenLessonDoesNotExist_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var fakeUser = CreateUserWithRole(userId, AppRoles.Admin);

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(fakeUser);
        _lessonRepo.FirstOrDefaultAsync(
                Arg.Any<LessonWithProjectionSpec<LessonDeletionInfo>>(), Arg.Any<CancellationToken>())
            .Returns((LessonDeletionInfo?)null);

        var command = new DeleteLessonCommand(1);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().Contain($"Lesson with id '{command.LessonId}' was not found");
    }

    [Fact]
    public async Task Handle_ValidRequest_DeletesLessonAndReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var fakeUser = CreateUserWithRole(userId, AppRoles.Teacher);
        var lessonInfo = CreateLessonInfo(id: 1);

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(fakeUser);
        _lessonRepo.FirstOrDefaultAsync(
                Arg.Any<LessonWithProjectionSpec<LessonDeletionInfo>>(), Arg.Any<CancellationToken>())
            .Returns(lessonInfo);

        var command = new DeleteLessonCommand(1);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NoContent);

        _lessonRepo.Received(1).Delete(Arg.Is<Lesson>(l => l.Id == 1));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLessonHasThumbnail_DeletesThumbnailFromStorage()
    {
        var userId = Guid.NewGuid();
        var fakeUser = CreateUserWithRole(userId, AppRoles.Teacher);
        var lessonInfo = CreateLessonInfo(id: 1, imageThumbnailUrl: "thumbnails/lesson-1.png");

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(fakeUser);
        _lessonRepo.FirstOrDefaultAsync(
                Arg.Any<LessonWithProjectionSpec<LessonDeletionInfo>>(), Arg.Any<CancellationToken>())
            .Returns(lessonInfo);

        var command = new DeleteLessonCommand(1);

        await _sut.Handle(command, CancellationToken.None);

        await _storageService.Received(1)
            .DeleteFileAsync("prisma", lessonInfo.ImageThumbnailUrl!, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLessonHasNoThumbnail_DoesNotCallStorageService()
    {
        var userId = Guid.NewGuid();
        var fakeUser = CreateUserWithRole(userId, AppRoles.Teacher);
        var lessonInfo = CreateLessonInfo(id: 1, imageThumbnailUrl: null);

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(fakeUser);
        _lessonRepo.FirstOrDefaultAsync(
                Arg.Any<LessonWithProjectionSpec<LessonDeletionInfo>>(), Arg.Any<CancellationToken>())
            .Returns(lessonInfo);

        var command = new DeleteLessonCommand(1);

        await _sut.Handle(command, CancellationToken.None);

        await _storageService.DidNotReceive()
            .DeleteFileAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSectionsHaveAssetIds_DeletesVideosFromVideoStorage()
    {
        var userId = Guid.NewGuid();
        var fakeUser = CreateUserWithRole(userId, AppRoles.Teacher);
        var lessonInfo = CreateLessonInfo(
            id: 1,
            sectionAssetIds: new List<string?> { "asset-123", null });

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(fakeUser);
        _lessonRepo.FirstOrDefaultAsync(
                Arg.Any<LessonWithProjectionSpec<LessonDeletionInfo>>(), Arg.Any<CancellationToken>())
            .Returns(lessonInfo);

        var command = new DeleteLessonCommand(1);

        await _sut.Handle(command, CancellationToken.None);

        await _videoStorageService.Received(1).DeleteVideoAsync("asset-123", Arg.Any<CancellationToken>());
        await _videoStorageService.Received(1).DeleteVideoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLessonHasEnrollments_DeletesEachEnrollmentByStub()
    {
        var userId = Guid.NewGuid();
        var fakeUser = CreateUserWithRole(userId, AppRoles.Teacher);
        var lessonInfo = CreateLessonInfo(id: 1, enrollmentIds: new List<int> { 10, 20 });

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(fakeUser);
        _lessonRepo.FirstOrDefaultAsync(
                Arg.Any<LessonWithProjectionSpec<LessonDeletionInfo>>(), Arg.Any<CancellationToken>())
            .Returns(lessonInfo);

        var command = new DeleteLessonCommand(1);

        await _sut.Handle(command, CancellationToken.None);

        await _enrollmentRepo.Received(1)
            .DeleteAsync(Arg.Is<Enrollment>(e => e.Id == 10), Arg.Any<CancellationToken>());
        await _enrollmentRepo.Received(1)
            .DeleteAsync(Arg.Is<Enrollment>(e => e.Id == 20), Arg.Any<CancellationToken>());
        _lessonRepo.Received(1).Delete(Arg.Is<Lesson>(l => l.Id == 1));
    }

    [Fact]
    public async Task Handle_WhenLessonHasNoEnrollments_DoesNotCallEnrollmentDelete()
    {
        var userId = Guid.NewGuid();
        var fakeUser = CreateUserWithRole(userId, AppRoles.Teacher);
        var lessonInfo = CreateLessonInfo(id: 1);

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(fakeUser);
        _lessonRepo.FirstOrDefaultAsync(
                Arg.Any<LessonWithProjectionSpec<LessonDeletionInfo>>(), Arg.Any<CancellationToken>())
            .Returns(lessonInfo);

        var command = new DeleteLessonCommand(1);

        await _sut.Handle(command, CancellationToken.None);

        await _enrollmentRepo.DidNotReceive()
            .DeleteAsync(Arg.Any<Enrollment>(), Arg.Any<CancellationToken>());
    }
}