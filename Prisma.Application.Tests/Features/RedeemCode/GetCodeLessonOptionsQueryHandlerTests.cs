using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.RedeemCodes.Queries.GetCodeLessonOptions;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.RedeemCodes;

namespace Prisma.Application.Tests.Features.RedeemCode;

public class GetCodeLessonOptionsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();

    private readonly IRepository<AcademicYearLesson, int>
        _repo = Substitute.For<IRepository<AcademicYearLesson, int>>();

    private readonly GetCodeLessonOptionsQueryHandler _sut;

    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly Guid _teacherId = Guid.NewGuid();

    public GetCodeLessonOptionsQueryHandlerTests()
    {
        _currentUser.UserId.Returns(_teacherId);
        _unitOfWork.GetOrCreateRepository<AcademicYearLesson, int>().Returns(_repo);
        _sut = new GetCodeLessonOptionsQueryHandler(_unitOfWork, _currentUserService, _identityService);
    }

    [Fact]
    public async Task Handle_WhenLinksExist_ReturnsDedupedLessons()
    {
        // Arrange — same lesson appears twice (two academic years) → should dedupe
        var links = new List<AcademicYearLesson>
        {
            new() { LessonId = 1, AcademicYearId = 1, Lesson = new() { Title = "الكهرباء الساكنة" } },
            new() { LessonId = 1, AcademicYearId = 1, Lesson = new() { Title = "الكهرباء الساكنة" } }, // duplicate
            new() { LessonId = 2, AcademicYearId = 1, Lesson = new() { Title = "قوانين نيوتن" } },
            new() { LessonId = 3, AcademicYearId = 2, Lesson = new() { Title = "المغناطيسية" } },
        };

        _repo.ListAsync(
                Arg.Any<TeacherAcademicYearLessonsSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns(links);

        // Act
        var result = await _sut.Handle(new GetCodeLessonOptionsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3); // duplicate removed
        result.Value.Should().ContainSingle(l => l.Name == "الكهرباء الساكنة");
        result.Value.Select(l => l.AcademicYearId).Should().BeEquivalentTo(new[] { 1, 1, 2 });
    }

    [Fact]
    public async Task Handle_WhenNoLinks_ReturnsEmptyList()
    {
        // Arrange
        _repo.ListAsync(
                Arg.Any<TeacherAcademicYearLessonsSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<AcademicYearLesson>());

        // Act
        var result = await _sut.Handle(new GetCodeLessonOptionsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUser.UserId.Returns((Guid?)null);

        // Act
        var result = await _sut.Handle(new GetCodeLessonOptionsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
    }
}