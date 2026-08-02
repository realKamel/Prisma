using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Sections.Commands.CreateSectionProgress;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Sections;
using Xunit;

namespace Prisma.Application.Tests.Features.Sections.Commands;

public class CreateSectionProgressCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IRepository<SectionProgress, int> _progressRepo = Substitute.For<IRepository<SectionProgress, int>>();
    private readonly CreateSectionProgressCommandHandler _sut;

    public CreateSectionProgressCommandHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<SectionProgress, int>().Returns(_progressRepo);
        _sut = new CreateSectionProgressCommandHandler(_unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new CreateSectionProgressCommand(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("User must be authenticated.");
    }

    [Fact]
    public async Task Handle_WhenProgressAlreadyExists_DoesNothing()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        _currentUserService.UserId.Returns(studentId);
        _progressRepo.AnyAsync(
                Arg.Any<SectionProgressByStudentAndSectionSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateSectionProgressCommand(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _progressRepo.DidNotReceive().Add(Arg.Any<SectionProgress>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProgressDoesNotExist_CreatesNewProgressAndSaves()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        _currentUserService.UserId.Returns(studentId);
        _progressRepo.AnyAsync(
                Arg.Any<SectionProgressByStudentAndSectionSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateSectionProgressCommand(5);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _progressRepo.Received(1).Add(Arg.Is<SectionProgress>(p =>
            p.SectionId == 5 &&
            p.StudentId == studentId &&
            p.IsCompleted == false &&
            p.WatchedSeconds == 0 &&
            p.Percentage == 0));

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}