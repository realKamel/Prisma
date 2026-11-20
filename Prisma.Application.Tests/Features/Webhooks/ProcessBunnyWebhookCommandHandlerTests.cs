using Ardalis.Specification;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Prisma.Application.Features.Webhooks.Bunny.Commands;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Tests.Features.Webhooks;

public class ProcessBunnyWebhookCommandHandlerTests
{
    private static readonly DateTimeOffset OccurredAt = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Video, int> _videoRepository = Substitute.For<
        IRepository<Video, int>
    >();
    private readonly ProcessBunnyWebhookCommandHandler _sut;

    public ProcessBunnyWebhookCommandHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Video, int>().Returns(_videoRepository);
        _sut = new ProcessBunnyWebhookCommandHandler(
            _unitOfWork,
            NullLogger<ProcessBunnyWebhookCommandHandler>.Instance
        );
    }

    private static ProcessBunnyWebhookCommand Command(VideoWebhookStatus status) =>
        new()
        {
            VideoLibraryId = 42,
            VideoGuid = Guid.NewGuid(),
            Status = status,
            EventId = Guid.NewGuid(),
            Timestamp = OccurredAt,
        };

    private static Video CreateVideo(VideoStatus? status = null) =>
        Video
            .Create(
                providerVideoId: Guid.NewGuid(),
                providerVideoLibraryId: 42,
                title: "Intro",
                storageSizeBytes: 0,
                durationInSeconds: 0,
                processingStatus: status ?? VideoStatus.Created
            )
            .Value;

    private void ReturnVideo(Video? video) =>
        _videoRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Video>>(), Arg.Any<CancellationToken>())
            .Returns(video);

    [Fact]
    public async Task Handle_WhenVideoIsUnknown_AcknowledgesDelivery()
    {
        ReturnVideo(null);

        var result = await _sut.Handle(
            Command(VideoWebhookStatus.Finished),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStatusIsNotMapped_AcknowledgesDelivery()
    {
        var video = CreateVideo();
        ReturnVideo(video);

        var result = await _sut.Handle(Command((VideoWebhookStatus)999), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        video.Status.Should().Be(VideoStatus.Created);
        video.LastProviderSyncAt.Should().Be(OccurredAt);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTransitionIsRejected_AcknowledgesDelivery()
    {
        var video = CreateVideo(VideoStatus.Finished);
        video.AcknowledgeProviderEvent(OccurredAt.AddMinutes(-1));
        ReturnVideo(video);

        var result = await _sut.Handle(
            Command(VideoWebhookStatus.Processing),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        video.Status.Should().Be(VideoStatus.Finished);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEventIsStatusOnly_AdvancesWatermarkAndSaves()
    {
        var video = CreateVideo();
        ReturnVideo(video);

        await _sut.Handle(Command(VideoWebhookStatus.CaptionsGenerated), CancellationToken.None);

        video.Status.Should().Be(VideoStatus.Created);
        video.LastProviderSyncAt.Should().Be(OccurredAt);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEventDeletesVideo_SoftDeletesAndSaves()
    {
        var video = CreateVideo(VideoStatus.Finished);
        video.AcknowledgeProviderEvent(OccurredAt.AddMinutes(-1));
        ReturnVideo(video);

        await _sut.Handle(Command(VideoWebhookStatus.VideoDeleted), CancellationToken.None);

        video.IsDeleted.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
