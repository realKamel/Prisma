using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Prisma.Application.Features.Lessons.Queries.GetLessonMaterialQuery;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Prisma.Domain.Specifications.Lessons;
using Xunit;

namespace Prisma.Application.Tests.Features.Lessons.Queries;

public class GetLessonMaterialQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    // عمل Mock للـ UserManager يحتاج تمرير IUserStore كمعامل أساسي في الـ Constructor لـ NSubstitute
    private readonly UserManager<User> _userManager = Substitute.For<UserManager<User>>(
        Substitute.For<IUserStore<User>>(), null, null, null, null, null, null, null, null);

    private readonly GetLessonMaterialQueryHandler _sut;

    public GetLessonMaterialQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);

        _sut = new GetLessonMaterialQueryHandler(_unitOfWork, _currentUserService, _userManager);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ReturnsUnauthorized()
    {
        // Arrange
        var query = new GetLessonMaterialQuery(1);
        _currentUserService.UserId.Returns((Guid?)null); // غير مسجل دخول

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("User must be authenticated.");
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExistInDatabase_ReturnsUnauthorized()
    {
        // Arrange
        var query = new GetLessonMaterialQuery(1);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        // محاكاة عدم العثور على أوبجكت المستخدم
        _userManager.FindByIdAsync(currentUserId.ToString()).Returns((User?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("User not found.");
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotHaveRequiredRole_ReturnsUnauthorized()
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
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("You are not authorized to view lesson materials.");
    }

    [Fact]
    public async Task Handle_WhenLessonDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var query = new GetLessonMaterialQuery(1);
        var currentUserId = Guid.NewGuid();
        var fakeUser = new User { Id = currentUserId };

        _currentUserService.UserId.Returns(currentUserId);
        _userManager.FindByIdAsync(currentUserId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Student }); // صلاحية طالب

        // محاكاة عدم وجود الدرس في قاعدة البيانات (FirstOrDefaultAsync بترجع null للـ List نفسها)
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonMaterialsSpecification>(), Arg.Any<CancellationToken>())
            .Returns((List<LessonMaterialProjection>?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
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

        // تجهيز مادة دراسية وهمية (كـ projection مباشرة، من غير DownloadUrl لأن الهاندلر بقى مبيستخدمش storage service)
        var baseDateTime = new DateTimeOffset(2026, 5, 10, 12, 0, 0, TimeSpan.Zero); // الساعة 12 بالتوقيت العالمي
        var fakeMaterials = new List<LessonMaterialProjection>
        {
            new()
            {
                Id = 501,
                Title = "ملخص القاعدة الأولى.pdf",
                Size = "2.5 MB",
                Type = "PDF",
                CreatedAt = baseDateTime
            }
        };

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonMaterialsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeMaterials);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull().And.HaveCount(1);

        var firstMaterial = result.Value[0];
        firstMaterial.Id.Should().Be(501);
        firstMaterial.Title.Should().Be("ملخص القاعدة الأولى.pdf");
        firstMaterial.Size.Should().Be("2.5 MB");
        firstMaterial.Type.Should().Be("PDF");

        // التحقق من صحة صياغة التاريخ وإضافة الـ 3 ساعات (12:00 + 3 ساعات = 15:00) بالتنسيق العربي
        var expectedDateString = baseDateTime.AddHours(3).ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("ar-EG"));
        firstMaterial.Date.Should().Be(expectedDateString);
    }

    [Fact]
    public async Task Handle_WhenLessonHasNoMaterials_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetLessonMaterialQuery(1);
        var currentUserId = Guid.NewGuid();
        var fakeUser = new User { Id = currentUserId };

        _currentUserService.UserId.Returns(currentUserId);
        _userManager.FindByIdAsync(currentUserId.ToString()).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { AppRoles.Teacher });

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonMaterialsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<LessonMaterialProjection>());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}