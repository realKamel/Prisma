using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Prisma.Application.Features.Lessons.Commands.UpdateLessonCommand;
using Prisma.Application.Features.Lessons.Commands.UpdateLessonDetails;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;


namespace Prisma.Application.Tests.Features.Lessons.Commands;

public class UpdateLessonDetailsCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly UserManager<User> _userManager;
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly IVideoStorageService _videoStorageService = Substitute.For<IVideoStorageService>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
    private readonly UpdateLessonDetailsCommandHandler _sut;

    public UpdateLessonDetailsCommandHandlerTests()
    {
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(store, null, null, null, null, null, null, null, null);

        _storageService.DefaultBucketName.Returns("prisma");
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _sut = new UpdateLessonDetailsCommandHandler(
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
        var command = CreateFakeUpdateCommand();

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("User must be authenticated.");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsUnauthorizedException()
    {
        var userId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns((User?)null);

        var command = CreateFakeUpdateCommand();

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("User not found.");
    }

    [Fact]
    public async Task Handle_WhenUserLacksRole_ThrowsUnauthorizedException()
    {
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { "Student" });

        var command = CreateFakeUpdateCommand();

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("Only teachers, assistants, and admins can modify lesson structures.");
    }

    [Fact]
    public async Task Handle_WhenLessonDoesNotExist_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Admin });
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<UpdateLessonDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Lesson?)null);

        var command = CreateFakeUpdateCommand();

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesLessonDetailsSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var lesson = new Lesson
        {
            Id = 1,
            Title = "Old Title",
            AcademicYears = new List<AcademicYearLesson>(),
            Sections = new List<Section>()
        };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<UpdateLessonDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(lesson);

        var command = CreateFakeUpdateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        lesson.Title.Should().Be("Updated Lesson");

        _lessonRepo.Received(1).Update(lesson);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenImageFileProvided_UploadsNewThumbnailAndQueuesOldForDeletion()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var lesson = new Lesson
        {
            Id = 1,
            AcademicYears = new List<AcademicYearLesson>(),
            Sections = new List<Section>(),
            ImageThumbnailUrl = "lessons/thumbnails/old-image.jpg"
        };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<UpdateLessonDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(lesson);

        var command = CreateFakeUpdateCommand();

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _storageService.Received(1).UploadFileAsync(
            "prisma", Arg.Any<string>(), Arg.Any<Stream>(), "image/jpeg", Arg.Any<CancellationToken>());
        await _storageService.Received(1)
            .DeleteFileAsync("prisma", "lessons/thumbnails/old-image.jpg", Arg.Any<CancellationToken>());
        lesson.ImageThumbnailUrl.Should().NotBe("lessons/thumbnails/old-image.jpg");
    }

    [Fact]
    public async Task Handle_WhenChapterRemoved_DeletesAssociatedVideoAsset()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var remainingSection = new Section { Id = 1, ContentURL = "kept.mp4", AssetId = "asset-kept" };
        var removedSection = new Section { Id = 2, ContentURL = "removed.mp4", AssetId = "asset-removed" };
        var lesson = new Lesson
        {
            Id = 1,
            AcademicYears = new List<AcademicYearLesson>(),
            Sections = new List<Section> { remainingSection, removedSection }
        };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<UpdateLessonDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(lesson);

        var command = CreateFakeUpdateCommand() with
        {
            Chapters = new List<ChapterCommandDto> { new("Kept Chapter", "kept.mp4") }
        };

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        lesson.Sections.Should().ContainSingle(s => s.ContentURL == "kept.mp4");
        await _videoStorageService.Received(1).DeleteVideoAsync("asset-removed", Arg.Any<CancellationToken>());
        await _videoStorageService.DidNotReceive().DeleteVideoAsync("asset-kept", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNewChapterAdded_CreatesNewSectionAndReturnsItInResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var lesson = new Lesson
        {
            Id = 1, AcademicYears = new List<AcademicYearLesson>(), Sections = new List<Section>()
        };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<UpdateLessonDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(lesson);

        var command = CreateFakeUpdateCommand() with
        {
            Chapters = new List<ChapterCommandDto> { new("Brand New Chapter", "new-video.mp4") }
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        lesson.Sections.Should().ContainSingle(s => s.ContentURL == "new-video.mp4");
        result.Value!.NewSections.Should().ContainSingle(s => s.ChapterIndex == 0);
    }

    [Fact]
    public async Task Handle_WhenAssignmentDisabled_RemovesAssignmentAndQueuesContentForDeletion()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var lesson = new Lesson
        {
            Id = 1,
            AcademicYears = new List<AcademicYearLesson>(),
            Sections = new List<Section>(),
            Assignment = new Assignment { Title = "Old Assignment", ContentURL = "assignments/old.pdf" }
        };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<UpdateLessonDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(lesson);

        var command = CreateFakeUpdateCommand() with { AssignmentEnabled = false };

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        lesson.Assignment.Should().BeNull();
        await _storageService.Received(1)
            .DeleteFileAsync("prisma", "assignments/old.pdf", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAcademicYearIdsChanged_AddsAndRemovesAccordingly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var lesson = new Lesson
        {
            Id = 1,
            Sections = new List<Section>(),
            AcademicYears = new List<AcademicYearLesson>
            {
                new() { AcademicYearId = 1, LessonId = 1 }, new() { AcademicYearId = 2, LessonId = 1 }
            }
        };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<UpdateLessonDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(lesson);

        var command = CreateFakeUpdateCommand() with { AcademicYearIds = new List<int> { 2, 3 } };

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        lesson.AcademicYears.Select(ay => ay.AcademicYearId).Should().BeEquivalentTo(new[] { 2, 3 });
    }

    private static UpdateLessonDetailsCommand CreateFakeUpdateCommand()
    {
        var mockImageFile = Substitute.For<IFormFile>();
        mockImageFile.FileName.Returns("new-image.jpg");
        mockImageFile.ContentType.Returns("image/jpeg");
        mockImageFile.Length.Returns(1024);
        mockImageFile.OpenReadStream().Returns(new MemoryStream());

        return new UpdateLessonDetailsCommand(
            Id: 1,
            Title: "Updated Lesson",
            Description: "New description",
            Price: 200.00m,
            PrerequisiteLessonId: null,
            Chapters: new List<ChapterCommandDto>(),
            AssignmentEnabled: true,
            AssignmentFile: null,
            AssignmentDueDate: null,
            IsPublished: true,
            AcademicYearIds: new List<int> { 1 },
            Outcomes: new List<string>(),
            ImageFile: mockImageFile
        );
    }
}