using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Lessons.Commands.CreateLessonDetails;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Prisma.Application.Tests.Features.Lessons.Commands;

public class CreateLessonDetailsCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly UserManager<User> _userManager;
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
    private readonly IRepository<AcademicYear, int> _academicYearRepo = Substitute.For<IRepository<AcademicYear, int>>();
    private readonly CreateLessonDetailsCommandHandler _sut;

    public CreateLessonDetailsCommandHandlerTests()
    {
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(store, null, null, null, null, null, null, null, null);

        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _unitOfWork.GetOrCreateRepository<AcademicYear, int>().Returns(_academicYearRepo);

        _sut = new CreateLessonDetailsCommandHandler(_unitOfWork, _currentUserService, _userManager, _storageService);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsUnauthorizedException()
    {
        var command = CreateFakeCommand();
        _currentUserService.UserId.Returns((Guid?)null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("User must be authenticated.");
    }

    [Fact]
    public async Task Handle_WhenPayloadIsValid_CreatesLessonAndReturnsSuccess()
    {
        // Arrange
        var command = CreateFakeCommand(academicYearIds: [1]);
        var userId = Guid.NewGuid();
        var fakeUser = new User { Id = userId };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });

        var validYears = new List<AcademicYear> { new() { Id = 1, Title = "الصف الأول" } };
        _academicYearRepo.ListAsync(Arg.Any<AcademicYearsByIdsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(validYears);

        // التعديل هنا: محاكاة تمرير الـ contentType في الـ Mock
        // قم بتعديل هذا الجزء في ملف CreateLessonDetailsCommandHandlerTests.cs
_storageService.UploadFileAsync(
    Arg.Any<string>(),             // 1. bucketName (string)
    Arg.Any<string>(),             // 2. objectKey (string)
    Arg.Any<Stream>(),             // 3. content (Stream)
    Arg.Any<string>(),             // 4. contentType (string)
    Arg.Any<CancellationToken>()   // 5. cancellationToken (CancellationToken)
)
.Returns(Task.FromResult("fake-storage-key")); // استخدم Task.FromResult لإرجاع القيمة بشكل صحيح

        _lessonRepo.When(x => x.Add(Arg.Any<Lesson>())).Do(call =>
        {
            var lesson = call.Arg<Lesson>();
            lesson.Id = 500;
            if (lesson.Sections != null && lesson.Sections.Any())
            {
                lesson.Sections.First().Id = 901;
            }
        });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.lessonId.Should().Be(500);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static CreateLessonDetailsCommand CreateFakeCommand(List<int>? academicYearIds = null)
    {
        var mockImageFile = Substitute.For<IFormFile>();
        mockImageFile.FileName.Returns("thumbnail.jpg");
        mockImageFile.ContentType.Returns("image/jpeg"); // إضافة الـ ContentType للـ Mock
        mockImageFile.OpenReadStream().Returns(new MemoryStream());

        var mockAssignmentFile = Substitute.For<IFormFile>();
        mockAssignmentFile.FileName.Returns("homework.pdf");
        mockAssignmentFile.ContentType.Returns("application/pdf"); // إضافة الـ ContentType للـ Mock
        mockAssignmentFile.OpenReadStream().Returns(new MemoryStream());

        return new CreateLessonDetailsCommand(
            Title: "درس القراءة والنصوص",
            Description: "شرح مفصل",
            Price: 120.00m,
            PrerequisiteLessonId: null,
            Chapters: [new ChapterCreateDto("الفصل الأول", "video.mp4")],
            AssignmentEnabled: true,
            AssignmentFile: mockAssignmentFile,
            AssignmentDueDate: DateTimeOffset.UtcNow.AddDays(5),
            IsPublished: true,
            AcademicYearIds: academicYearIds ?? [1],
            Outcomes: ["فهم القواعد"],
            ImageFile: mockImageFile
        );
    }
}