using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Lessons.Queries.GetLessonStatus;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Xunit;

namespace Prisma.Application.Tests.Features.Lessons.Queries;

public class GetLessonStatusQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly GetLessonStatusQueryHandler _sut;

    public GetLessonStatusQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _sut = new GetLessonStatusQueryHandler(_unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        var query = new GetLessonStatusQuery(1);
        _currentUserService.UserId.Returns((Guid?)null); // غير مسجل دخول

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WhenLessonDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var query = new GetLessonStatusQuery(1);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonStatusSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Lesson)null);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNoEnrollmentExists_ReturnsAvailableStatus()
    {
        // Arrange
        var lessonId = 1;
        var query = new GetLessonStatusQuery(lessonId);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        var fakeLesson = new Lesson
        {
            Id = lessonId,
            Enrollments = new List<Enrollment>() // قائمة تسجيلات فارغة (الطالب لم يشتريه بعد)
        };

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonStatusSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Status.Should().Be(LessonCatalogStatus.Available);
    }

    [Fact]
    public async Task Handle_WhenEnrollmentIsExpired_ReturnsExpiredStatus()
    {
        // Arrange
        var lessonId = 1;
        var query = new GetLessonStatusQuery(lessonId);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        var fakeLesson = new Lesson
        {
            Id = lessonId,
            Enrollments = new List<Enrollment>
            {
                new()
                {
                    StudentId = currentUserId,
                    ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1) // منتهي الصلاحية منذ يوم
                }
            }
        };

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonStatusSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Status.Should().Be(LessonCatalogStatus.Expired);
    }

    [Fact]
    public async Task Handle_WhenPrerequisiteIsNotCompleted_ReturnsLockedStatus()
    {
        // Arrange
        var lessonId = 1;
        var query = new GetLessonStatusQuery(lessonId);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        // تجهيز الدرس المتطلب وبداخله تسجيل الطالب غير المكتمل
        var fakePrerequisiteLesson = new Lesson
        {
            Id = 99,
            Enrollments = new List<Enrollment>
            {
                new() { StudentId = currentUserId, IsCompleted = false } // لم يكتمل بعد!
            }
        };

        var fakeLesson = new Lesson
        {
            Id = lessonId,
            Prerequisite = fakePrerequisiteLesson,
            Enrollments = new List<Enrollment>
            {
                new() { StudentId = currentUserId, ExpiresAt = DateTimeOffset.UtcNow.AddDays(5) } // مسجل وصالح
            }
        };

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonStatusSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Status.Should().Be(LessonCatalogStatus.Locked);
    }

    [Fact]
    public async Task Handle_WhenEnrollmentIsValidAndPrerequisitesMet_ReturnsPurchasedStatus()
    {
        // Arrange
        var lessonId = 1;
        var query = new GetLessonStatusQuery(lessonId);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        // حالة نجاح: متطلب سابق مكتمل
        var fakePrerequisiteLesson = new Lesson
        {
            Id = 99,
            Enrollments = new List<Enrollment>
            {
                new() { StudentId = currentUserId, IsCompleted = true } // مكتمل بنجاح
            }
        };

        var fakeLesson = new Lesson
        {
            Id = lessonId,
            Prerequisite = fakePrerequisiteLesson,
            Enrollments = new List<Enrollment>
            {
                new() { StudentId = currentUserId, ExpiresAt = DateTimeOffset.UtcNow.AddDays(5) } // التسجيل صالح وممتد
            }
        };

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonStatusSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Status.Should().Be(LessonCatalogStatus.Purchased);
    }
}