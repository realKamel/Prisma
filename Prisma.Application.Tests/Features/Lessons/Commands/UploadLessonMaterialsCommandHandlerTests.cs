using Ardalis.Result;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Lessons.Commands.UploadLessonMaterialsCommand;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Tests.Features.Lessons.Commands;

public class UploadLessonMaterialsCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly ICurrentUserService _currentUserService =
        Substitute.For<ICurrentUserService>();

    // private readonly UserManager<User> identityService;
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();

    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<
        IRepository<Lesson, int>
    >();

    private readonly UploadLessonMaterialsCommandHandler _sut;

    public UploadLessonMaterialsCommandHandlerTests()
    {
        _storageService.DefaultBucketName.Returns("prisma");
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _sut = new UploadLessonMaterialsCommandHandler(
            _unitOfWork,
            _currentUserService,
            _identityService,
            _storageService
        );
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ReturnsUnauthorized()
    {
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new UploadLessonMaterialsCommand(1, new List<IFormFile>());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("User must be authenticated.");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsUnauthorized()
    {
        var userId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId).Returns((User?)null);

        var command = new UploadLessonMaterialsCommand(1, new List<IFormFile>());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("User not found.");
    }

    [Fact]
    public async Task Handle_WhenUserLacksRole_ReturnsUnauthorized()
    {
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId).Returns(fakeUser);
        _identityService.GetRolesAsync(fakeUser).Returns(new List<string> { "Student" });

        var command = new UploadLessonMaterialsCommand(1, new List<IFormFile>());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result
            .Errors.Should()
            .Contain("Only teachers and assistants can upload materials to lessons.");
    }

    [Fact]
    public async Task Handle_WhenLessonDoesNotExist_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId).Returns(fakeUser);
        _identityService.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });

        // ?? ??????? ???????? Specification ?????? ??????? ?? ????????
        _lessonRepo
            .FirstOrDefaultAsync(
                Arg.Any<LessonWithMaterialsForUpdateSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns((Lesson?)null);

        var command = new UploadLessonMaterialsCommand(1, new List<IFormFile>());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenNoFilesProvided_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var lesson = new Lesson { Id = 1, LessonMaterials = new List<LessonMaterial>() };

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId).Returns(fakeUser);
        _identityService.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });

        _lessonRepo
            .FirstOrDefaultAsync(
                Arg.Any<LessonWithMaterialsForUpdateSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(lesson);

        var command = new UploadLessonMaterialsCommand(1, new List<IFormFile>());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain("No files provided for upload.");
    }

    [Fact]
    public async Task Handle_ValidRequest_UploadsFilesAndAddsToLesson()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var lesson = new Lesson { Id = 1, LessonMaterials = new List<LessonMaterial>() };

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId).Returns(fakeUser);
        _identityService.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });

        _lessonRepo
            .FirstOrDefaultAsync(
                Arg.Any<LessonWithMaterialsForUpdateSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(lesson);

        var mockFile = Substitute.For<IFormFile>();
        mockFile.FileName.Returns("lecture.pdf");
        mockFile.Length.Returns(1024);
        mockFile.ContentType.Returns("application/pdf");
        mockFile.OpenReadStream().Returns(new MemoryStream());

        var command = new UploadLessonMaterialsCommand(1, new List<IFormFile> { mockFile });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Materials uploaded and attached to lesson successfully");

        lesson.LessonMaterials.Should().ContainSingle();
        var firstMaterial = lesson.LessonMaterials.First();
        firstMaterial.Title.Should().Be("lecture");
        firstMaterial.Size.Should().Be("1 KB");
        firstMaterial.LessonId.Should().Be(lesson.Id);

        await _storageService
            .Received(1)
            .UploadFileAsync(
                "prisma",
                Arg.Any<string>(),
                Arg.Any<Stream>(),
                "application/pdf",
                Arg.Any<CancellationToken>()
            );
        _lessonRepo.Received(1).Update(lesson);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenFileHasZeroLength_SkipsFileAndDoesNotUpload()
    {
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var lesson = new Lesson { Id = 1, LessonMaterials = new List<LessonMaterial>() };

        _currentUserService.UserId.Returns(userId);
        _identityService.FindByIdAsync(userId).Returns(fakeUser);
        _identityService.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });

        _lessonRepo
            .FirstOrDefaultAsync(
                Arg.Any<LessonWithMaterialsForUpdateSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(lesson);

        var emptyFile = Substitute.For<IFormFile>();
        emptyFile.FileName.Returns("empty.pdf");
        emptyFile.Length.Returns(0);

        var command = new UploadLessonMaterialsCommand(1, new List<IFormFile> { emptyFile });

        await _sut.Handle(command, CancellationToken.None);

        lesson.LessonMaterials.Should().BeEmpty();
        await _storageService
            .DidNotReceive()
            .UploadFileAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Stream>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            );
    }
}