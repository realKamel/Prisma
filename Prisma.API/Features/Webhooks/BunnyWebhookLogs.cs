namespace Prisma.API.Features.Webhooks;

/// <summary>
/// Edge-of-system log messages for the Bunny webhook endpoint. These fire before the
/// payload reaches the application layer, so they use their own event-id range.
/// </summary>
internal static partial class BunnyWebhookLogs
{
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Warning,
        Message = "Bunny webhook: unknown library {LibraryId} from {RemoteIp}"
    )]
    public static partial void UnknownLibrary(ILogger logger, long libraryId, string? remoteIp);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Error,
        Message = "Bunny webhook: library {LibraryId} has no read-only API key — webhooks cannot be verified"
    )]
    public static partial void MissingSigningKey(ILogger logger, long libraryId);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Warning,
        Message = "Bunny webhook: signature verification failed for library {LibraryId} from {RemoteIp}"
    )]
    public static partial void SignatureVerificationFailed(
        ILogger logger,
        long libraryId,
        string? remoteIp
    );

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Warning,
        Message = "Bunny webhook: invalid JSON body for library {LibraryId} from {RemoteIp}"
    )]
    public static partial void InvalidJson(
        ILogger logger,
        long libraryId,
        string? remoteIp,
        Exception exception
    );

    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Warning,
        Message = "Bunny webhook: malformed payload for library {LibraryId} from {RemoteIp}"
    )]
    public static partial void MalformedPayload(ILogger logger, long libraryId, string? remoteIp);

    [LoggerMessage(
        EventId = 2006,
        Level = LogLevel.Error,
        Message = "Bunny webhook: library id mismatch — route {RouteLibraryId}, payload {PayloadLibraryId}, event {EventId}"
    )]
    public static partial void LibraryIdMismatch(
        ILogger logger,
        long routeLibraryId,
        long payloadLibraryId,
        Guid eventId
    );

    [LoggerMessage(
        EventId = 2007,
        Level = LogLevel.Information,
        Message = "Bunny webhook: ignored live stream event {EventId} for library {LibraryId}"
    )]
    public static partial void LiveStreamEventIgnored(ILogger logger, Guid eventId, long libraryId);

    [LoggerMessage(
        EventId = 2008,
        Level = LogLevel.Error,
        Message = "Bunny webhook: handler rejected {EventStatus} event {EventId} for library {LibraryId}"
    )]
    public static partial void CommandRejected(
        ILogger logger,
        string eventStatus,
        Guid eventId,
        long libraryId
    );
}
