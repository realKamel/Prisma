using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Features.Storage.Commands.MuxWebhook;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Xunit;

namespace Prisma.Application.Tests.Features.Storage.Commands;

public class MuxWebhookCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Section, int> _sectionRepo = Substitute.For<IRepository<Section, int>>();
    private readonly MuxWebhookCommandHandler _sut;

    public MuxWebhookCommandHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Section, int>().Returns(_sectionRepo);
        _sut = new MuxWebhookCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenSectionDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _sectionRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Section?)null);
        var command = new MuxWebhookCommand("asset-123", "playback-456", 1);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesAssetAndPlaybackIdAndSaves()
    {
        // Arrange
        var section = new Section { Id = 1, AssetId = null, PlaybackId = null };
        _sectionRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(section);

        var command = new MuxWebhookCommand("asset-123", "playback-456", 1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        section.AssetId.Should().Be("asset-123");
        section.PlaybackId.Should().Be("playback-456");

        _sectionRepo.Received(1).Update(section);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSectionAlreadyHasAssetInfo_OverwritesExistingValues()
    {
        // Arrange
        var section = new Section { Id = 1, AssetId = "old-asset", PlaybackId = "old-playback" };
        _sectionRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(section);

        var command = new MuxWebhookCommand("new-asset", "new-playback", 1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        section.AssetId.Should().Be("new-asset");
        section.PlaybackId.Should().Be("new-playback");
    }
}