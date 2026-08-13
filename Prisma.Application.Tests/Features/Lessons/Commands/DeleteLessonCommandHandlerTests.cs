using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Prisma.Application.Features.Lessons.Commands.DeleteLessonCommand;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Xunit;

namespace Prisma.Application.Tests.Features.Lessons.Commands;

public class DeleteLessonCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly IVideoStorageService _videoStorageService = Substitute.For<IVideoStorageService>();
    private readonly DeleteLessonCommandHandler _sut;

    public DeleteLessonCommandHandlerTests()
    {
        _storageService.DefaultBucketName.Returns("prisma");
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
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
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = CreateUserWithRole(userId, "Student");

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(fakeUser);

        var command = new DeleteLessonCommand(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenLessonDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = CreateUserWithRole(userId, AppRoles.Admin);

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(fakeUser);
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonWithEnrollmentSpec>(), Arg.Any<CancellationToken>())
            .Returns((Lesson?)null);

        var command = new DeleteLessonCommand(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().Contain($"Lesson with id '{command.LessonId}' was not found");
    }

    [Fact]
    public async Task Handle_ValidRequest_DeletesLessonAndReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = CreateUserWithRole(userId, AppRoles.Teacher);
        var fakeLesson = new Lesson { Id = 1 };

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(fakeUser);
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonWithEnrollmentSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        var command = new DeleteLessonCommand(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NoContent);

        _lessonRepo.Received(1).Delete(fakeLesson);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLessonHasThumbnail_DeletesThumbnailFromStorage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = CreateUserWithRole(userId, AppRoles.Teacher);
        var fakeLesson = new Lesson { Id = 1, ImageThumbnailUrl = "thumbnails/lesson-1.png" };

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(fakeUser);
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonWithEnrollmentSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        var command = new DeleteLessonCommand(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _storageService.Received(1)
            .DeleteFileAsync("prisma", fakeLesson.ImageThumbnailUrl, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLessonHasNoThumbnail_DoesNotCallStorageService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = CreateUserWithRole(userId, AppRoles.Teacher);
        var fakeLesson = new Lesson { Id = 1, ImageThumbnailUrl = null };

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(fakeUser);
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonWithEnrollmentSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        var command = new DeleteLessonCommand(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _storageService.DidNotReceive()
            .DeleteFileAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSectionsHaveAssetIds_DeletesVideosFromVideoStorage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = CreateUserWithRole(userId, AppRoles.Teacher);
        var sectionWithAsset = new Section { Id = 1, AssetId = "asset-123" };
        var sectionWithoutAsset = new Section { Id = 2, AssetId = null };
        var fakeLesson = new Lesson
        {
            Id = 1,
            Sections = new List<Section> { sectionWithAsset, sectionWithoutAsset }
        };

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(fakeUser);
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonWithEnrollmentSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        var command = new DeleteLessonCommand(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _videoStorageService.Received(1).DeleteVideoAsync("asset-123", Arg.Any<CancellationToken>());
        await _videoStorageService.Received(1).DeleteVideoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLessonHasEnrollments_ClearsEnrollmentsBeforeDeleting()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = CreateUserWithRole(userId, AppRoles.Teacher);
        var fakeLesson = new Lesson
        {
            Id = 1,
            Enrollments = new List<Enrollment> { new Enrollment() }
        };

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(fakeUser);
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonWithEnrollmentSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        var command = new DeleteLessonCommand(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        fakeLesson.Enrollments.Should().BeEmpty();
        _lessonRepo.Received(1).Delete(fakeLesson);
    }
}