using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.RedeemCodes.Dtos;
using Prisma.Application.Features.RedeemCodes.Queries.GetCodeLessonOptions;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.RedeemCodes;

namespace Prisma.Application.Tests.Features.RedeemCode;

public class GetCodeLessonOptionsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly Guid _teacherId = Guid.NewGuid();

    private readonly IRepository<Prisma.Domain.Entities.LessonAggregate.AcademicYearLesson, int> _repo =
        Substitute.For<IRepository<Prisma.Domain.Entities.LessonAggregate.AcademicYearLesson, int>>();

    private readonly GetCodeLessonOptionsQueryHandler _sut;

    public GetCodeLessonOptionsQueryHandlerTests()
    {
        _currentUserService.UserId.Returns(_teacherId);
        _identityService
            .FindByIdAsync(_teacherId, Arg.Any<CancellationToken>())
            .Returns(new Prisma.Domain.Entities.UserAggregate.Teacher { Id = _teacherId });
        _unitOfWork
            .GetOrCreateRepository<Prisma.Domain.Entities.LessonAggregate.AcademicYearLesson, int>()
            .Returns(_repo);
        _sut = new GetCodeLessonOptionsQueryHandler(_unitOfWork, _currentUserService, _identityService);
    }

    [Fact]
    public async Task Handle_WhenLinksExist_ReturnsDedupedLessons()
    {
        // Arrange — same lesson appears twice (two academic years) → should dedupe
        var links = new List<CodeLessonOptionDto>
        {
            new() { Id = 1, AcademicYearId = 1, Name = "الكهرباء الساكنة" },
            new() { Id = 1, AcademicYearId = 1, Name = "الكهرباء الساكنة" }, // duplicate
            new() { Id = 2, AcademicYearId = 1, Name = "قوانين نيوتن" },
            new() { Id = 3, AcademicYearId = 2, Name = "المغناطيسية" },
        };

        _repo.ListAsync(
                Arg.Any<TeacherAcademicYearLessonsSpecification<CodeLessonOptionDto>>(),
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
                Arg.Any<TeacherAcademicYearLessonsSpecification<CodeLessonOptionDto>>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<CodeLessonOptionDto>());

        // Act
        var result = await _sut.Handle(new GetCodeLessonOptionsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        // Act
        var result = await _sut.Handle(new GetCodeLessonOptionsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        _identityService
            .FindByIdAsync(_teacherId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _sut.Handle(new GetCodeLessonOptionsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}