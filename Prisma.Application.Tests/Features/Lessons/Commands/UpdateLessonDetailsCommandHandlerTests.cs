using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Lessons.Commands.UpdateLessonCommand;
using Prisma.Application.Features.Lessons.Commands.UpdateLessonDetails;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Xunit;

namespace Prisma.Application.Tests.Features.Lessons.Commands;

public class UpdateLessonDetailsCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly UserManager<User> _userManager;
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
    private readonly UpdateLessonDetailsCommandHandler _sut;

    public UpdateLessonDetailsCommandHandlerTests()
    {
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(store, null, null, null, null, null, null, null, null);

        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _sut = new UpdateLessonDetailsCommandHandler(_unitOfWork, _currentUserService, _userManager, _storageService);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthorized_ThrowsUnauthorizedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);
        var command = CreateFakeUpdateCommand();

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesLessonDetailsSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };
        var lesson = new Lesson { Id = 1, Title = "Old Title", AcademicYears = new List<AcademicYearLesson>() };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });

         _lessonRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(lesson);
        await _storageService.UploadFileAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

        var command = CreateFakeUpdateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        lesson.Title.Should().Be("Updated Lesson");

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static UpdateLessonDetailsCommand CreateFakeUpdateCommand()
    {
        var mockImageFile = Substitute.For<IFormFile>();
        mockImageFile.FileName.Returns("new-image.jpg");
        mockImageFile.ContentType.Returns("image/jpeg");
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