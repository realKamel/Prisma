using FluentAssertions;
using NSubstitute;
using Ardalis.Result;
using Prisma.Application.Features.Lessons.Queries.GetLessonEditorDetails;
using Prisma.Application.Features.Lessons.Queries.GetLessonFormOptions;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Prisma.Application.Abstractions.Services;
using Xunit;

namespace Prisma.Application.Tests.Features.Lessons.Queries;

public class GetLessonFormOptionsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();

    private readonly GetLessonFormOptionsQueryHandler _sut;
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    public GetLessonFormOptionsQueryHandlerTests()
    {
        // إعداد الـ Repositories داخل الـ Unit of Work الوهمي
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);

        _sut = new GetLessonFormOptionsQueryHandler(_unitOfWork, _identityService, _currentUserService);
    }

    private Guid SetupAuthenticatedTeacher()
    {
        var teacherId = Guid.NewGuid();
        _currentUserService.UserId.Returns(teacherId);
        _identityService
            .FindByIdAsync(teacherId, Arg.Any<CancellationToken>())
            .Returns(new Prisma.Domain.Entities.UserAggregate.Teacher { Id = teacherId });

        return teacherId;
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        // Act
        var result = await _sut.Handle(new GetLessonFormOptionsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        await _lessonRepo
            .DidNotReceive()
            .ListAsync(Arg.Any<LessonWithProjectionSpec<LessonDto>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId);
        _identityService
            .FindByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _sut.Handle(new GetLessonFormOptionsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenOptionsExist_ReturnsMappedPrerequisitesOptions()
    {
        // Arrange
        SetupAuthenticatedTeacher();
        var query = new GetLessonFormOptionsQuery();

        var fakeLessons = new List<LessonDto>
        {
            new("درس القراءة: الحرية", 10),
            new("درس النحو: المبتدأ والخبر", 11),
        };

        _lessonRepo
            .ListAsync(Arg.Any<LessonWithProjectionSpec<LessonDto>>(), Arg.Any<CancellationToken>())
            .Returns(fakeLessons);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PrerequisitesOptions.Should().HaveCount(2);
        result.Value.PrerequisitesOptions.Should().ContainInOrder(
            new LessonDto("درس القراءة: الحرية", 10),
            new LessonDto("درس النحو: المبتدأ والخبر", 11)
        );
    }

    [Fact]
    public async Task Handle_WhenNoOptionsExist_ReturnsEmptyOptionsList()
    {
        // Arrange
        SetupAuthenticatedTeacher();
        var query = new GetLessonFormOptionsQuery();

        _lessonRepo
            .ListAsync(Arg.Any<LessonWithProjectionSpec<LessonDto>>(), Arg.Any<CancellationToken>())
            .Returns(new List<LessonDto>());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // التأكد من أن القائمة فارغة تماماً وليست Null لضمان عدم حدوث مشاكل في فرونت إند
        result.Value.PrerequisitesOptions.Should().BeEmpty();
    }
}