using Ardalis.Result;
using Microsoft.Extensions.Logging;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.VideoSpecs;

namespace Prisma.Application.Features.Webhooks.Bunny.Commands;

public sealed partial class ProcessBunnyWebhookCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<ProcessBunnyWebhookCommandHandler> logger
) : ICommandHandler<ProcessBunnyWebhookCommand, Result>
{
    public async Task<Result> Handle(
        ProcessBunnyWebhookCommand request,
        CancellationToken cancellationToken
    )
    {
        var occurredAt = request.Timestamp;

        if (occurredAt == default)
        {
            // A missing timestamp would make every later delivery look stale, so fall back to
            // arrival time and surface the anomaly instead.
            occurredAt = DateTimeOffset.UtcNow;
            MissingTimestamp(logger, request.VideoGuid, request.EventId);
        }

        var videoRepository = unitOfWork.GetOrCreateRepository<Video, int>();

        var video = await videoRepository.FirstOrDefaultAsync(
            new VideoByProviderId(request.VideoLibraryId, request.VideoGuid),
            cancellationToken
        );

        if (video is null)
        {
            // Already removed locally or never one of ours. Retrying cannot help, so the
            // delivery is acknowledged rather than failed.
            VideoNotFound(logger, request.VideoLibraryId, request.VideoGuid, request.EventId);
            return Result.Success();
        }

        if (IsStatusOnlyEvent(request.Status))
        {
            video.AcknowledgeProviderEvent(occurredAt);
            StatusOnlyEventAcknowledged(logger, request.Status.ToString(), request.VideoGuid);
        }
        else
        {
            var transition = ApplyTransition(video, request.Status, occurredAt, request.Reason);

            if (transition is null)
            {
                // A status the provider added after this build. The watermark still advances so
                // a later, older event cannot overwrite newer state; the log is the signal that
                // the mapping needs extending.
                video.AcknowledgeProviderEvent(occurredAt);
                UnknownStatus(logger, (int)request.Status, request.VideoGuid, request.EventId);
            }
            else if (!transition.IsSuccess)
            {
                // Rejected by the aggregate (regression or terminal state) - permanent, so it
                // must not be retried by the provider.
                TransitionRejected(
                    logger,
                    requestedStatus: request.Status.ToString(),
                    currentStatus: video.Status.Name,
                    request.VideoGuid,
                    request.EventId
                );

                return Result.Success();
            }
            else
            {
                Accepted(
                    logger,
                    request.Status.ToString(),
                    video.Status.Name,
                    request.VideoGuid,
                    request.VideoLibraryId
                );
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static Result? ApplyTransition(
        Video video,
        VideoWebhookStatus status,
        DateTimeOffset occurredAt,
        string? reason
    )
    {
        return status switch
        {
            VideoWebhookStatus.Queued
            or VideoWebhookStatus.Processing
            or VideoWebhookStatus.Encoding
            or VideoWebhookStatus.ResolutionFinished => video.MarkProcessingStarted(occurredAt),

            VideoWebhookStatus.PresignedUploadFinished => video.MarkUploaded(occurredAt),

            VideoWebhookStatus.Finished => video.MarkFinished(occurredAt),

            VideoWebhookStatus.Failed => video.MarkFailed(
                isUploadFailure: false,
                occurredAt: occurredAt,
                reason: reason
            ),

            VideoWebhookStatus.PresignedUploadFailed => video.MarkFailed(
                isUploadFailure: true,
                occurredAt: occurredAt,
                reason: reason
            ),

            VideoWebhookStatus.VideoDeleted => video.MarkDeleted(occurredAt),

            _ => null,
        };
    }

    /// <summary>
    /// Events the provider emits alongside encoding that do not change the processing state.
    /// </summary>
    private static bool IsStatusOnlyEvent(VideoWebhookStatus status) =>
        status
            is VideoWebhookStatus.PresignedUploadStarted
                or VideoWebhookStatus.CaptionsGenerated
                or VideoWebhookStatus.TitleOrDescriptionGenerated
                or VideoWebhookStatus.CaptionGenerationFailed
                or VideoWebhookStatus.ChaptersGenerated
                or VideoWebhookStatus.MomentsGenerated
                or VideoWebhookStatus.SmartGenerationFailed;

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Warning,
        Message = "Bunny webhook: no local video {VideoId} in library {LibraryId} for event {EventId} — delivery ignored"
    )]
    private static partial void VideoNotFound(
        ILogger logger,
        long libraryId,
        Guid videoId,
        Guid eventId
    );

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Warning,
        Message = "Bunny webhook: rejected {RequestedStatus} for video {VideoId} (currently {CurrentStatus}), event {EventId} — delivery ignored"
    )]
    private static partial void TransitionRejected(
        ILogger logger,
        string requestedStatus,
        string currentStatus,
        Guid videoId,
        Guid eventId
    );

    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Warning,
        Message = "Bunny webhook: unhandled status {StatusCode} for video {VideoId}, event {EventId} — delivery ignored"
    )]
    private static partial void UnknownStatus(
        ILogger logger,
        int statusCode,
        Guid videoId,
        Guid eventId
    );

    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Information,
        Message = "Bunny webhook: accepted {EventStatus} for video {VideoId} in library {LibraryId}, video is now {VideoStatus}"
    )]
    private static partial void Accepted(
        ILogger logger,
        string eventStatus,
        string videoStatus,
        Guid videoId,
        long libraryId
    );

    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Debug,
        Message = "Bunny webhook: acknowledged {EventStatus} for video {VideoId} without a status change"
    )]
    private static partial void StatusOnlyEventAcknowledged(
        ILogger logger,
        string eventStatus,
        Guid videoId
    );

    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Warning,
        Message = "Bunny webhook: event {EventId} for video {VideoId} has no timestamp — using arrival time"
    )]
    private static partial void MissingTimestamp(ILogger logger, Guid videoId, Guid eventId);
}
