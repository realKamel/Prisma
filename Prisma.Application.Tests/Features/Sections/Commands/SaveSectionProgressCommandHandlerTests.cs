using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Sections.Commands.SaveSectionProgress;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Sections;

namespace Prisma.Application.Tests.Features.Sections.Commands;

public class SaveSectionProgressCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService =
        Substitute.For<ICurrentUserService>();
    private readonly IRepository<SectionProgress, int> _progressRepo = Substitute.For<
        IRepository<SectionProgress, int>
    >();
    private readonly IRepository<Section, int> _sectionRepo = Substitute.For<
        IRepository<Section, int>
    >();
    private readonly SaveSectionProgressCommandHandler _sut;

    public SaveSectionProgressCommandHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<SectionProgress, int>().Returns(_progressRepo);
        _unitOfWork.GetOrCreateRepository<Section, int>().Returns(_sectionRepo);
        _sut = new SaveSectionProgressCommandHandler(_unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new SaveSectionProgressCommand(1, 30);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenProgressDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        _currentUserService.UserId.Returns(studentId);
        _progressRepo
            .FirstOrDefaultAsync(
                Arg.Any<SectionProgressByStudentAndSectionSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns((SectionProgress?)null);

        var command = new SaveSectionProgressCommand(1, 30);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenSectionProjectionIsMissing_StillUpdatesWatchedSeconds()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var progress = new SectionProgress { Id = 1 };

        _currentUserService.UserId.Returns(studentId);
        _progressRepo
            .FirstOrDefaultAsync(
                Arg.Any<SectionProgressByStudentAndSectionSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(progress);
        _sectionRepo
            .FirstOrDefaultAsync(
                Arg.Any<SectionWithProjectionSpec<long>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(0L);

        var command = new SaveSectionProgressCommand(1, 30);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        // NOTE: the handler does not guard against a missing section; it divides by the
        // default 0 duration, so only the watched seconds are meaningfully updated here.
        result.IsSuccess.Should().BeTrue();
        progress.WatchedSeconds.Should().Be(30);
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesWatchedSecondsAndComputesPercentage()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var progress = new SectionProgress
        {
            Id = 1,
            WatchedSeconds = 0,
            Percentage = 0,
        };

        _currentUserService.UserId.Returns(studentId);
        _progressRepo
            .FirstOrDefaultAsync(
                Arg.Any<SectionProgressByStudentAndSectionSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(progress);
        _sectionRepo
            .FirstOrDefaultAsync(
                Arg.Any<SectionWithProjectionSpec<long>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(200L);

        var command = new SaveSectionProgressCommand(1, 100);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        progress.WatchedSeconds.Should().Be(100);
        progress.Percentage.Should().Be(50);

        _progressRepo.Received(1).Update(progress);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWatchedSecondsExceedsDuration_PercentageExceeds100()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var progress = new SectionProgress { Id = 1 };

        _currentUserService.UserId.Returns(studentId);
        _progressRepo
            .FirstOrDefaultAsync(
                Arg.Any<SectionProgressByStudentAndSectionSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(progress);
        _sectionRepo
            .FirstOrDefaultAsync(
                Arg.Any<SectionWithProjectionSpec<long>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(60L);

        var command = new SaveSectionProgressCommand(1, 90);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        // NOTE: this documents current behavior (150%), not necessarily desired behavior — see flag below.
        progress.Percentage.Should().Be(150);
    }
}
