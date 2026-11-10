using Ardalis.Result;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.ObjectStorage;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
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
    private readonly IIdentityService _userManager;
    private readonly IStorageService _storageService;
    private readonly IRepository<Lesson, int> _lessonRepo;
    private readonly IRepository<AcademicYear, int> _academicYearRepo;
    private readonly CreateLessonDetailsCommandHandler _handler;

    public CreateLessonDetailsCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _storageService = Substitute.For<IStorageService>();
        _storageService.DefaultBucketName.Returns("prisma");
        _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
        _academicYearRepo = Substitute.For<IRepository<AcademicYear, int>>();
        _userManager = Substitute.For<IIdentityService>();

        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _unitOfWork.GetOrCreateRepository<AcademicYear, int>().Returns(_academicYearRepo);

        _currentUserService.UserId.Returns(Guid.NewGuid());

        _handler = new CreateLessonDetailsCommandHandler(
            _unitOfWork,
            _currentUserService,
            _userManager,
            _storageService
        );
    }

    private static IFormFile CreateMockFile(string fileName, string contentType)
    {
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns(fileName);
        file.ContentType.Returns(contentType);
        file.Length.Returns(100);
        file.OpenReadStream().Returns(new MemoryStream());
        return file;
    }

    private static CreateLessonDetailsCommand CreateCommand(
        string title = "title",
        string? description = "desc",
        decimal price = 10m,
        string currency = "EGP",
        int? prerequisiteLessonId = null,
        IList<ChapterCreateDto>? chapters = null,
        bool assignmentEnabled = false,
        IFormFile? assignmentFile = null,
        DateTimeOffset? assignmentDueDate = null,
        bool isPublished = true,
        IList<int>? academicYearIds = null,
        IList<string>? outcomes = null,
        IFormFile? imageFile = null,
        Guid? teacherId = null,
        long validityDurationInSeconds = 0
    ) =>
        new(
            title,
            description,
            price,
            currency,
            prerequisiteLessonId,
            chapters,
            assignmentEnabled,
            assignmentFile,
            assignmentDueDate,
            isPublished,
            academicYearIds,
            outcomes,
            imageFile,
            teacherId,
            validityDurationInSeconds
        );

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ReturnsUnauthorized()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        _lessonRepo.DidNotReceive().Add(Arg.Any<Lesson>());
        await _userManager
            .DidNotReceive()
            .FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotFoundInIdentity_ReturnsUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId).Returns((User?)null);

        // Act
        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        _lessonRepo.DidNotReceive().Add(Arg.Any<Lesson>());
    }

    [Fact]
    public async Task Handle_WhenUserLacksRequiredRole_ReturnsUnauthorized()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        _currentUserService.UserId.Returns(user.Id);
        _userManager.FindByIdAsync(Arg.Any<Guid>()).Returns(user);
        _userManager.GetRolesAsync(user).Returns(new List<string> { AppRoles.Student });

        // Act
        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("Only teachers and assistants can create lessons.");
    }

    [Fact]
    public async Task Handle_WhenAssistantHasNoTeacherAssignment_ReturnsError()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        _currentUserService.UserId.Returns(user.Id);
        _userManager.FindByIdAsync(Arg.Any<Guid>()).Returns(user);
        _userManager.GetRolesAsync(user).Returns(new List<string> { AppRoles.Assistant });

        // Act
        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain("Assistant record is missing teacher assignment.");
    }

    [Fact]
    public async Task Handle_WhenAdminOmitsTeacher_ReturnsError()
    {
        // Arrange
        var admin = new User { Id = EnvAdminId };
        _currentUserService.UserId.Returns(EnvAdminId);
        _userManager.FindByIdAsync(EnvAdminId).Returns(admin);
        _userManager.GetRolesAsync(admin).Returns(new List<string> { AppRoles.Admin });

        // Act
        var result = await _handler.Handle(CreateCommand(teacherId: null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain("Admin-created lessons require an explicit teacher.");
    }

    [Fact]
    public async Task Handle_WhenAdminSpecifiesUnknownTeacher_ReturnsError()
    {
        // Arrange
        var admin = new User { Id = EnvAdminId };
        _currentUserService.UserId.Returns(EnvAdminId);
        var unknownTeacherId = Guid.NewGuid();
        _userManager.FindByIdAsync(EnvAdminId).Returns(admin);
        _userManager.GetRolesAsync(admin).Returns(new List<string> { AppRoles.Admin });
        _userManager.FindByIdAsync(unknownTeacherId).Returns(new User { Id = unknownTeacherId });

        // Act
        var result = await _handler.Handle(
            CreateCommand(teacherId: unknownTeacherId),
            CancellationToken.None
        );

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain("Specified teacher does not exist.");
    }

    [Fact]
    public async Task Handle_WhenAcademicYearsAreInvalid_ReturnsError()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        var teacher = new Prisma.Domain.Entities.UserAggregate.Teacher { Id = teacherId };
        _currentUserService.UserId.Returns(teacherId);
        _userManager.FindByIdAsync(teacherId).Returns(teacher);
        _userManager.GetRolesAsync(teacher).Returns(new List<string> { AppRoles.Teacher });

        var command = CreateCommand(academicYearIds: new List<int> { 1, 2 });

        // Simulating DB returning fewer matching years than requested (e.g. invalid IDs)
        _academicYearRepo
            .ListAsync(Arg.Any<AcademicYearsByIdsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<AcademicYear> { new() { Id = 1 } });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain("invalid academic year");
        _lessonRepo.DidNotReceive().Add(Arg.Any<Lesson>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HappyPath_CreatesLessonWithAllAssetsAndDetails()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        var assistant = new Assistant { Id = Guid.NewGuid(), TeacherId = teacherId };
        _currentUserService.UserId.Returns(assistant.Id);
        _userManager.FindByIdAsync(assistant.Id).Returns(assistant);
        _userManager.GetRolesAsync(assistant).Returns(new List<string> { AppRoles.Assistant });

        var mockImage = CreateMockFile("thumbnail.jpg", "image/jpeg");
        var mockAssignment = CreateMockFile("homework.pdf", "application/pdf");

        var command = CreateCommand(
            title: "Calculus I",
            description: "Introductory Calculus",
            price: 150m,
            prerequisiteLessonId: 42,
            chapters: new List<ChapterCreateDto>
            {
                new("Limits Intro", "vid1.mp4", 100),
                new("Derivatives Intro", "vid2.mp4", 400),
            },
            assignmentEnabled: true,
            assignmentFile: mockAssignment,
            assignmentDueDate: DateTimeOffset.UtcNow.AddDays(5),
            isPublished: true,
            academicYearIds: new List<int> { 10 },
            outcomes: new List<string> { "Learn Limits", "Learn Derivatives" },
            imageFile: mockImage,
            validityDurationInSeconds: 3600
        );

        _academicYearRepo
            .ListAsync(Arg.Any<AcademicYearsByIdsSpecification>(), Arg.Any<CancellationToken>())
            .Returns([new() { Id = 10 }]);

        Lesson? savedLesson = null;
        _lessonRepo.Add(Arg.Do<Lesson>(l => savedLesson = l));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.lessonId.Should().Be(savedLesson!.Id);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _lessonRepo.Received(1).Add(Arg.Any<Lesson>());

        // Structural Validation on Intercepted Domain Entity
        savedLesson.Should().NotBeNull();
        savedLesson!.Title.Should().Be("Calculus I");
        savedLesson.Description.Should().Be("Introductory Calculus");
        savedLesson.Price.Should().Be(150m);
        savedLesson.Money.Amount.Should().Be(150m);
        savedLesson.Money.Currency.Should().Be("EGP");
        savedLesson.PrerequisiteId.Should().Be(42);
        savedLesson.Status.Should().Be(LessonStatus.Active);
        savedLesson.TeacherId.Should().Be(teacherId);
        savedLesson.Outcomes.Should().ContainInOrder("Learn Limits", "Learn Derivatives");
        savedLesson.ValidityDuration.Seconds.Should().Be(3600);

        // Thumbnail Assertions
        savedLesson
            .ImageThumbnailUrl.Should()
            .StartWith("lessons/thumbnails/")
            .And.EndWith(".jpg");
        await _storageService
            .Received(1)
            .UploadFileAsync(
                "prisma",
                savedLesson.ImageThumbnailUrl!,
                Arg.Any<Stream>(),
                "image/jpeg",
                Arg.Any<CancellationToken>()
            );

        // Chapter Order Assertions
        savedLesson.Sections.Should().HaveCount(2);
        savedLesson.Sections.ElementAt(0).Title.Should().Be("Limits Intro");
        savedLesson.Sections.ElementAt(0).SortOrder.Should().Be(1);
        savedLesson.Sections.ElementAt(0).Duration.Should().Be(TimeSpan.FromSeconds(100));
        savedLesson.Sections.ElementAt(1).Title.Should().Be("Derivatives Intro");
        savedLesson.Sections.ElementAt(1).SortOrder.Should().Be(2);
        savedLesson.Sections.ElementAt(1).Duration.Should().Be(TimeSpan.FromSeconds(400));
        savedLesson.TimeDuration.Seconds.Should().Be(500);

        // Assignment Assertions
        savedLesson.Assignment.Should().NotBeNull();
        savedLesson.Assignment!.Title.Should().Be("homework");
        savedLesson.Assignment.ContentURL.Should().StartWith("assignments/").And.EndWith(".pdf");
        await _storageService
            .Received(1)
            .UploadFileAsync(
                "prisma",
                savedLesson.Assignment.ContentURL,
                Arg.Any<Stream>(),
                "application/pdf",
                Arg.Any<CancellationToken>()
            );

        // Academic Year Assertions
        savedLesson.AcademicYears.Should().HaveCount(1);
        savedLesson.AcademicYears.Single().AcademicYearId.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WhenOptionalFieldsAreNull_DefaultsCorrectly()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        var teacher = new Prisma.Domain.Entities.UserAggregate.Teacher { Id = teacherId };
        _currentUserService.UserId.Returns(teacherId);
        _userManager.FindByIdAsync(teacherId).Returns(teacher);
        _userManager.GetRolesAsync(teacher).Returns(new List<string> { AppRoles.Teacher });

        var command = CreateCommand(
            title: "Draft Lesson",
            description: "Introductory Calculus",
            price: 150m,
            prerequisiteLessonId: 42,
            chapters: null,
            isPublished: false,
            academicYearIds: null,
            outcomes: null,
            imageFile: null
        );

        Lesson? savedLesson = null;
        _lessonRepo.Add(Arg.Do<Lesson>(l => savedLesson = l));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        savedLesson.Should().NotBeNull();
        savedLesson!.Status.Should().Be(LessonStatus.Drafted);
        savedLesson.TeacherId.Should().Be(teacherId);
        savedLesson.Outcomes.Should().BeEmpty();
        savedLesson.Sections.Should().BeEmpty();
        savedLesson.AcademicYears.Should().BeEmpty();
        savedLesson.ImageThumbnailUrl.Should().BeNull();
        savedLesson.Assignment.Should().BeNull();
        savedLesson.TimeDuration.Seconds.Should().Be(0);
    }

    private static readonly Guid EnvAdminId = Guid.NewGuid();
}
