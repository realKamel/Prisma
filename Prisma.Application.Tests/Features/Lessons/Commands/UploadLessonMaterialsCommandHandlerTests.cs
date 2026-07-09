using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Lessons.Commands.UploadLessonMaterials;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Prisma.Application.Tests.Features.Lessons.Commands;

public class UploadLessonMaterialsCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly UserManager<User> _userManager;
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
    private readonly UploadLessonMaterialsCommandHandler _sut;

    public UploadLessonMaterialsCommandHandlerTests()
    {
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(store, null, null, null, null, null, null, null, null);

        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _sut = new UploadLessonMaterialsCommandHandler(_unitOfWork, _currentUserService, _userManager, _storageService);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsUnauthorizedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new UploadLessonMaterialsCommand(1, new List<IFormFile>());

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("User must be authenticated.");
    }

    [Fact]
    public async Task Handle_WhenUserLacksRole_ThrowsUnauthorizedException()
    {
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { "Student" });

        var command = new UploadLessonMaterialsCommand(1, new List<IFormFile>());

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Only teachers and assistants can upload materials to lessons.");
    }

    [Fact]
    public async Task Handle_WhenLessonDoesNotExist_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonMaterialsSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Lesson?)null);

        var command = new UploadLessonMaterialsCommand(1, new List<IFormFile>());

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNoFilesProvided_ThrowsBadRequestException()
    {
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var lesson = new Lesson { Id = 1, LessonMaterials = new List<LessonMaterial>() };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonMaterialsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(lesson);

        var command = new UploadLessonMaterialsCommand(1, new List<IFormFile>());

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_ValidRequest_UploadsFilesAndAddsToLesson()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var lesson = new Lesson { Id = 1, LessonMaterials = new List<LessonMaterial>() };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonMaterialsSpecification>(), Arg.Any<CancellationToken>())
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
        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be("Materials uploaded and attached to lesson successfully");

        lesson.LessonMaterials.Should().ContainSingle();
        var firstMaterial = lesson.LessonMaterials.First();
        firstMaterial.Title.Should().Be("lecture");
        firstMaterial.Size.Should().Be("1 KB");
        firstMaterial.LessonId.Should().Be(lesson.Id);

        await _storageService.Received(1).UploadFileAsync(
            "prisma", Arg.Any<string>(), Arg.Any<Stream>(), "application/pdf", Arg.Any<CancellationToken>());
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
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonMaterialsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(lesson);

        var emptyFile = Substitute.For<IFormFile>();
        emptyFile.FileName.Returns("empty.pdf");
        emptyFile.Length.Returns(0);

        var command = new UploadLessonMaterialsCommand(1, new List<IFormFile> { emptyFile });

        await _sut.Handle(command, CancellationToken.None);

        lesson.LessonMaterials.Should().BeEmpty();
        await _storageService.DidNotReceive().UploadFileAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}