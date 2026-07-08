using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Lessons.Queries.GetLessonMaterialQuery;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Prisma.Application.Tests.Features.Lessons.Queries;

public class GetLessonMaterialQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();

    // عمل Mock للـ UserManager يحتاج تمرير IUserStore كمعامل أساسي في الـ Constructor لـ NSubstitute
    private readonly UserManager<User> _userManager = Substitute.For<UserManager<User>>(
        Substitute.For<IUserStore<User>>(), null, null, null, null, null, null, null, null);

    private readonly GetLessonMaterialQueryHandler _sut;

    public GetLessonMaterialQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);

        _sut = new GetLessonMaterialQueryHandler(_unitOfWork, _currentUserService, _userManager, _storageService);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsUnauthorizedException()
    {
        // Arrange
        var query = new GetLessonMaterialQuery(1);
        _currentUserService.UserId.Returns((Guid?)null); // غير مسجل دخول

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("User must be authenticated.");
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExistInDatabase_ThrowsUnauthorizedException()
    {
        // Arrange
        var query = new GetLessonMaterialQuery(1);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        // محاكاة عدم العثور على أوبجكت المستخدم
        _userManager.FindByIdAsync(currentUserId.ToString()).Returns((User)null);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotHaveRequiredRole_ThrowsUnauthorizedException()
    {
        // Arrange
        var query = new GetLessonMaterialQuery(1);
        var currentUserId = Guid.NewGuid();
        var fakeUser = new User { Id = currentUserId };

        _currentUserService.UserId.Returns(currentUserId);
        _userManager.FindByIdAsync(currentUserId.ToString()).Returns(fakeUser);

        // إرجاع دور غير مصرح له برؤية المواد (مثلاً دور زائر أو فارغ)
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { "Guest" });

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("You are not authorized to view lesson materials.");
    }

    [Fact]
    public async Task Handle_WhenLessonDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var query = new GetLessonMaterialQuery(1);
        var currentUserId = Guid.NewGuid();
        var fakeUser = new User { Id = currentUserId };

        _currentUserService.UserId.Returns(currentUserId);
        _userManager.FindByIdAsync(currentUserId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Student }); // صلاحية طالب

        // محاكاة عدم وجود الدرس في قاعدة البيانات
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonMaterialsSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Lesson)null);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenLessonExistsAndUserAuthorized_ReturnsMappedLessonMaterialDtos()
    {
        // Arrange
        var lessonId = 1;
        var query = new GetLessonMaterialQuery(lessonId);
        var currentUserId = Guid.NewGuid();
        var fakeUser = new User { Id = currentUserId };

        _currentUserService.UserId.Returns(currentUserId);
        _userManager.FindByIdAsync(currentUserId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Student });

        // تجهيز مادتين دراسيتين وهميتين وتحديد الـ CreatedAt
        var baseDateTime = new DateTimeOffset(2026, 5, 10, 12, 0, 0, TimeSpan.Zero); // الساعة 12 بالتوقيت العالمي
        var fakeLesson = new Lesson
        {
            Id = lessonId,
            LessonMaterials = new List<LessonMaterial>
            {
                new()
                {
                    Id = 501,
                    Title = "ملخص القاعدة الأولى.pdf",
                    Size = "2.5 MB",
                    Type = LessonMaterialType.PDF, // بافتراض وجود هذا الـ Enum لديك
                    DownloadUrl = "materials/pdf1.pdf",
                    CreatedAt = baseDateTime
                }
            }
        };

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonMaterialsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        // عمل Mock للـ Storage Service لترجع رابط وهمي عند استدعائها
        _storageService.GetDownloadUrlAsync("prisma", "materials/pdf1.pdf").Returns(Task.FromResult("https://download.prisma.com/pdf1.pdf"));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull().And.HaveCount(1);

        var firstMaterial = result.Data[0];
        firstMaterial.Id.Should().Be(501);
        firstMaterial.Title.Should().Be("ملخص القاعدة الأولى.pdf");
        firstMaterial.Size.Should().Be("2.5 MB");
        firstMaterial.Type.Should().Be(LessonMaterialType.PDF.ToString());

        // التحقق من صحة صياغة التاريخ وإضافة الـ 3 ساعات (12:00 + 3 ساعات = 15:00) بالتنسيق العربي
        var expectedDateString = baseDateTime.AddHours(3).ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("ar-EG"));
        firstMaterial.Date.Should().Be(expectedDateString);
    }
}