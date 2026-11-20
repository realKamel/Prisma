using FluentAssertions;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Application.Tests.Features.Videos;

public class VideoTransitionTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

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

    [Fact]
    public void MarkFinished_FromCreated_IsApplied()
    {
        var video = CreateVideo();

        var result = video.MarkFinished(Start);

        result.IsSuccess.Should().BeTrue();
        video.Status.Should().Be(VideoStatus.Finished);
        video.LastProviderSyncAt.Should().Be(Start);
    }

    [Fact]
    public void MarkProcessingStarted_WhenAlreadyFinished_IsRejected()
    {
        var video = CreateVideo(VideoStatus.Finished);
        video.AcknowledgeProviderEvent(Start);

        var result = video.MarkProcessingStarted(Start.AddMinutes(1));

        result.IsSuccess.Should().BeFalse();
        video.Status.Should().Be(VideoStatus.Finished);
        video.LastProviderSyncAt.Should().Be(Start);
    }

    [Fact]
    public void MarkFinished_WithOlderEventThanLastSync_IsIgnored()
    {
        var video = CreateVideo();
        video.MarkProcessingStarted(Start);

        var result = video.MarkFinished(Start.AddMinutes(-5));

        result.IsSuccess.Should().BeTrue();
        video.Status.Should().Be(VideoStatus.Processing);
        video.LastProviderSyncAt.Should().Be(Start);
    }

    [Fact]
    public void MarkFinished_CalledTwiceWithSameTimestamp_IsIdempotent()
    {
        var video = CreateVideo();

        video.MarkFinished(Start);
        var result = video.MarkFinished(Start);

        result.IsSuccess.Should().BeTrue();
        video.Status.Should().Be(VideoStatus.Finished);
        video.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void MarkDeleted_WhenFinished_SoftDeletesTheVideo()
    {
        var video = CreateVideo(VideoStatus.Finished);
        video.AcknowledgeProviderEvent(Start);
        var deletedAt = Start.AddMinutes(10);

        var result = video.MarkDeleted(deletedAt);

        result.IsSuccess.Should().BeTrue();
        video.Status.Should().Be(VideoStatus.Deleted);
        video.IsDeleted.Should().BeTrue();
        video.DeletedAt.Should().Be(deletedAt);
    }

    [Fact]
    public void MarkDeleted_WithStaleTimestamp_LeavesTheVideoIntact()
    {
        var video = CreateVideo(VideoStatus.Finished);
        video.AcknowledgeProviderEvent(Start);

        var result = video.MarkDeleted(Start.AddMinutes(-1));

        result.IsSuccess.Should().BeTrue();
        video.Status.Should().Be(VideoStatus.Finished);
        video.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void AcknowledgeProviderEvent_BlocksOlderStatusEvents()
    {
        var video = CreateVideo();
        video.MarkProcessingStarted(Start.AddMinutes(5));

        video.AcknowledgeProviderEvent(Start.AddMinutes(10));

        var result = video.MarkFinished(Start.AddMinutes(7));

        result.IsSuccess.Should().BeTrue();
        video.Status.Should().Be(VideoStatus.Processing);
    }
}
