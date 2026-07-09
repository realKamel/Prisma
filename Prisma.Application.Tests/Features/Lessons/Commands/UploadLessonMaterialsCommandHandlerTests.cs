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
using System;
using System.Collections.Generic;
using System.IO;
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
    public async Task Handle_ValidRequest_UploadsFilesAndAddsToLesson()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var lesson = new Lesson { Id = 1, LessonMaterials = new List<LessonMaterial>() };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });

        // محاكاة الحصول على الدرس
        _lessonRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(lesson);

        // محاكاة ملفات الـ Upload
        var mockFile = Substitute.For<IFormFile>();
        mockFile.FileName.Returns("lecture.pdf");
        mockFile.Length.Returns(1024);
        mockFile.ContentType.Returns("application/pdf");
        mockFile.OpenReadStream().Returns(new MemoryStream());

        var command = new UploadLessonMaterialsCommand(1, new List<IFormFile> { mockFile });

        // تصحيح استدعاء خدمة التخزين
        _storageService.UploadFileAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Stream>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>()
        ).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be("Materials uploaded and attached to lesson successfully");

        lesson.LessonMaterials.Should().ContainSingle();
        var firstMaterial = lesson.LessonMaterials.FirstOrDefault();
        firstMaterial.Title.Should().Be("lecture"); // تم إزالة الامتداد

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}