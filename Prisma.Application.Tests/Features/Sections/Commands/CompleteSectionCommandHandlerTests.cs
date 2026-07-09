using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Sections.Commands.CompleteSection;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Sections;
using Xunit;

namespace Prisma.Application.Tests.Features.Sections.Commands;

public class CompleteSectionCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IRepository<SectionProgress, int> _progressRepo = Substitute.For<IRepository<SectionProgress, int>>();
    private readonly CompleteSectionCommandHandler _sut;

    public CompleteSectionCommandHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<SectionProgress, int>().Returns(_progressRepo);
        _sut = new CompleteSectionCommandHandler(_unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new CompleteSectionCommand(1);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("User must be authenticated.");
    }

    [Fact]
    public async Task Handle_WhenProgressDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        _currentUserService.UserId.Returns(studentId);
        _progressRepo.FirstOrDefaultAsync(
                Arg.Any<SectionProgressByStudentAndSectionSpecification>(), Arg.Any<CancellationToken>())
            .Returns((SectionProgress?)null);

        var command = new CompleteSectionCommand(1);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ValidRequest_MarksProgressCompleteAndSaves()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var progress = new SectionProgress { Id = 1, IsCompleted = false, Percentage = 40 };

        _currentUserService.UserId.Returns(studentId);
        _progressRepo.FirstOrDefaultAsync(
                Arg.Any<SectionProgressByStudentAndSectionSpecification>(), Arg.Any<CancellationToken>())
            .Returns(progress);

        var command = new CompleteSectionCommand(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        progress.IsCompleted.Should().BeTrue();
        progress.Percentage.Should().Be(100);

        _progressRepo.Received(1).Update(progress);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}