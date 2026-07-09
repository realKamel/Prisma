using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Lessons.Commands.DeleteLessonCommand;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Xunit;

namespace Prisma.Application.Tests.Features.Lessons.Commands;

public class DeleteLessonCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly UserManager<User> _userManager;
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly IVideoStorageService _videoStorageService = Substitute.For<IVideoStorageService>();
    private readonly DeleteLessonCommandHandler _sut;

    public DeleteLessonCommandHandlerTests()
    {
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(store, null, null, null, null, null, null, null, null);

        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _sut = new DeleteLessonCommandHandler(
            _unitOfWork,
            _currentUserService,
            _userManager,
            _storageService,
            _videoStorageService);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsUnauthorizedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new DeleteLessonCommand(1);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("User must be authenticated to delete a lesson.");
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotHavePermission_ThrowsUnauthorizedException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        // مستخدم ليس لديه أدوار (Teacher/Assistant/Admin)
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { "Student" });

        var command = new DeleteLessonCommand(1);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Only teachers, assistants, and admins can delete lessons");
    }

    [Fact]
    public async Task Handle_WhenLessonDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Admin });
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonWithEnrollmentSpec>(), Arg.Any<CancellationToken>())
            .Returns((Lesson?)null);

        var command = new DeleteLessonCommand(1);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ValidRequest_DeletesLessonAndReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var fakeLesson = new Lesson { Id = 1 };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonWithEnrollmentSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        var command = new DeleteLessonCommand(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be("Lesson deleted successfully");

        _lessonRepo.Received(1).Delete(fakeLesson);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLessonHasThumbnail_DeletesThumbnailFromStorage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var fakeLesson = new Lesson { Id = 1, ImageThumbnailUrl = "thumbnails/lesson-1.png" };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonWithEnrollmentSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        var command = new DeleteLessonCommand(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _storageService.Received(1).DeleteFileAsync("prisma", fakeLesson.ImageThumbnailUrl);
    }

    [Fact]
    public async Task Handle_WhenLessonHasNoThumbnail_DoesNotCallStorageService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var fakeLesson = new Lesson { Id = 1, ImageThumbnailUrl = null };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonWithEnrollmentSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        var command = new DeleteLessonCommand(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _storageService.DidNotReceive().DeleteFileAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenSectionsHaveAssetIds_DeletesVideosFromVideoStorage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var sectionWithAsset = new Section { Id = 1, AssetId = "asset-123" };
        var sectionWithoutAsset = new Section { Id = 2, AssetId = null };
        var fakeLesson = new Lesson
        {
            Id = 1,
            Sections = new List<Section> { sectionWithAsset, sectionWithoutAsset }
        };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonWithEnrollmentSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        var command = new DeleteLessonCommand(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _videoStorageService.Received(1).DeleteVideoAsync("asset-123");
        await _videoStorageService.Received(1).DeleteVideoAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenLessonHasEnrollments_ClearsEnrollmentsBeforeDeleting()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var fakeLesson = new Lesson
        {
            Id = 1,
            Enrollments = new List<Enrollment> { new Enrollment() }
        };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });
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