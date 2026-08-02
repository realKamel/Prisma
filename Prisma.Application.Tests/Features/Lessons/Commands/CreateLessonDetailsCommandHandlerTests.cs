using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.BackgroundJobs;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Prisma.Application.Features.Lessons.Commands.CreateLessonDetails;
using Prisma.Application.Features.Lessons.Commands.CreateLessonDetailsCommand;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Tests.Features.Lessons.Commands;

public class CreateLessonDetailsCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<User> _userManager;
    private readonly IStorageService _storageService;
    private readonly IRepository<Lesson, int> _lessonRepo;
    private readonly IRepository<AcademicYear, int> _academicYearRepo;
    private readonly CreateLessonDetailsCommandHandler _handler;
    private readonly IBackgroundJobService _backgroundJobService;

    public CreateLessonDetailsCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _storageService = Substitute.For<IStorageService>();
        _storageService.DefaultBucketName.Returns("prisma");
        _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
        _academicYearRepo = Substitute.For<IRepository<AcademicYear, int>>();
        _backgroundJobService = Substitute.For<IBackgroundJobService>();
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(store, null!, null!, null!, null!, null!, null!, null!, null!);

        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _unitOfWork.GetOrCreateRepository<AcademicYear, int>().Returns(_academicYearRepo);

        _currentUserService.UserId.Returns(Guid.NewGuid());

        _handler = new CreateLessonDetailsCommandHandler(_unitOfWork, _currentUserService, _userManager,
            _storageService, _backgroundJobService);
    }

    private IFormFile CreateMockFile(string fileName, string contentType)
    {
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns(fileName);
        file.ContentType.Returns(contentType);
        file.Length.Returns(100);
        file.OpenReadStream().Returns(new MemoryStream());
        return file;
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(
            new CreateLessonDetailsCommand(
                "title",
                "desc",
                10,
                null,
                new List<ChapterCreateDto>(),
                false,
                null,
                null,
                true,
                new List<int>(),
                new List<string>(),
                null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("User must be authenticated.");
    }

    [Fact]
    public async Task Handle_WhenUserNotFoundInIdentity_ThrowsUnauthorizedException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns((User?)null);

        // Act
        var result = await _handler.Handle(new CreateLessonDetailsCommand(
            "title",
            "desc",
            10,
            null,
            new List<ChapterCreateDto>(),
            false,
            null,
            null,
            true,
            new List<int>(),
            new List<string>(),
            null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("User not found.");
    }

    [Fact]
    public async Task Handle_WhenUserLacksRequiredRole_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        _userManager.FindByIdAsync(Arg.Any<string>()).Returns(user);
        _userManager.GetRolesAsync(user)
            .Returns(new List<string> { AppRoles.Student }); // Missing Teacher/Assistant/Admin

        // Act
        var result = await _handler.Handle(new CreateLessonDetailsCommand(
            "title",
            "desc",
            10,
            null,
            new List<ChapterCreateDto>(),
            false,
            null,
            null,
            true,
            new List<int>(),
            new List<string>(),
            null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("Only teachers and assistants can create lessons.");
    }

    [Fact]
    public async Task Handle_WhenAcademicYearsAreInvalid_ThrowsBadRequestException()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        _userManager.FindByIdAsync(Arg.Any<string>()).Returns(user);
        _userManager.GetRolesAsync(user).Returns(new List<string> { AppRoles.Teacher });

        var command = new CreateLessonDetailsCommand
        ("Maths",
            "desc",
            10,
            null,
            new List<ChapterCreateDto>(),
            false,
            null,
            null,
            true,
            new List<int> { 1, 2 },
            new List<string>(),
            null);

        // Simulating DB returning fewer matching years than requested (e.g. invalid IDs)
        _academicYearRepo.ListAsync(Arg.Any<AcademicYearsByIdsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<AcademicYear> { new() { Id = 1 } });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain("invalid academic year");
    }

    [Fact]
    public async Task Handle_HappyPath_CreatesLessonWithAllAssetsAndDetails()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        _userManager.FindByIdAsync(Arg.Any<string>()).Returns(user);
        _userManager.GetRolesAsync(user).Returns(new List<string> { AppRoles.Assistant });

        var mockImage = CreateMockFile("thumbnail.jpg", "image/jpeg");
        var mockAssignment = CreateMockFile("homework.pdf", "application/pdf");

        var command = new CreateLessonDetailsCommand
        (
            "Calculus I",
            "Introductory Calculus",
            150m,
            42,
            new List<ChapterCreateDto>
            {
                new ChapterCreateDto("Limits Intro", "vid1.mp4"),
                new ChapterCreateDto("Derivatives Intro", "vid2.mp4")
            },
            true,
            mockAssignment,
            DateTimeOffset.UtcNow.AddDays(5),
            true,
            new List<int> { 10 },
            new List<string> { "Learn Limits", "Learn Derivatives" },
            mockImage
        );

        _academicYearRepo.ListAsync(Arg.Any<AcademicYearsByIdsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<AcademicYear> { new() { Id = 10 } });

        Lesson? savedLesson = null;
        _lessonRepo.Add(Arg.Do<Lesson>(l => savedLesson = l));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _lessonRepo.Received(1).Add(Arg.Any<Lesson>());

        // Structural Validation on Intercepted Domain Entity
        savedLesson.Should().NotBeNull();
        savedLesson!.Title.Should().Be("Calculus I");
        savedLesson.Description.Should().Be("Introductory Calculus");
        savedLesson.Price.Should().Be(150m);
        savedLesson.PrerequisiteId.Should().Be(42);
        savedLesson.Status.Should().Be(LessonStatus.Active);
        savedLesson.Outcomes.Should().ContainInOrder("Learn Limits", "Learn Derivatives");

        // Thumbnail Assertions
        savedLesson.ImageThumbnailUrl.Should().StartWith("lessons/thumbnails/").And.EndWith(".jpg");
        await _storageService.Received(1).UploadFileAsync("prisma", savedLesson.ImageThumbnailUrl!, Arg.Any<Stream>(),
            "image/jpeg", Arg.Any<CancellationToken>());

        // Chapter Order Assertions
        savedLesson.Sections.Should().HaveCount(2);
        savedLesson.Sections.ElementAt(0).Title.Should().Be("Limits Intro");
        savedLesson.Sections.ElementAt(0).SortOrder.Should().Be(1);
        savedLesson.Sections.ElementAt(1).Title.Should().Be("Derivatives Intro");
        savedLesson.Sections.ElementAt(1).SortOrder.Should().Be(2);

        // Assignment Assertions
        savedLesson.Assignment.Should().NotBeNull();
        savedLesson.Assignment!.Title.Should().Be("homework");
        savedLesson.Assignment.ContentURL.Should().StartWith("assignments/").And.EndWith(".pdf");
        await _storageService.Received(1).UploadFileAsync("prisma", savedLesson.Assignment.ContentURL,
            Arg.Any<Stream>(), "application/pdf", Arg.Any<CancellationToken>());

        // Academic Year Assertions
        savedLesson.AcademicYears.Should().HaveCount(1);
        savedLesson.AcademicYears.Single().AcademicYearId.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WhenOptionalFieldsAreNull_DefaultsCorrectly()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        _userManager.FindByIdAsync(Arg.Any<string>()).Returns(user);
        _userManager.GetRolesAsync(user).Returns(new List<string> { AppRoles.Admin });
        var command = new CreateLessonDetailsCommand
        (
            "Draft Lesson",
            "Introductory Calculus",
            150m,
            42,
            null,
            false,
            null,
            null,
            false,
            null,
            null,
            null
        );


        Lesson? savedLesson = null;
        _lessonRepo.Add(Arg.Do<Lesson>(l => savedLesson = l));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        savedLesson.Should().NotBeNull();
        savedLesson!.Status.Should().Be(LessonStatus.Drafted);
        savedLesson.Outcomes.Should().BeEmpty();
        savedLesson.Sections.Should().BeEmpty();
        savedLesson.AcademicYears.Should().BeEmpty();
        savedLesson.ImageThumbnailUrl.Should().BeNull();
        savedLesson.Assignment.Should().BeNull();
    }
}