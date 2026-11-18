using Ardalis.SmartEnum;

namespace Prisma.Domain.Entities.LessonAggregate;

public sealed class VideoStatus : SmartEnum<VideoStatus>
{
    public static readonly VideoStatus Created = new(
        nameof(Created),
        0,
        isTerminal: false,
        isFailure: false,
        isReadyForPlayback: false
    );

    public static readonly VideoStatus Uploaded = new(
        nameof(Uploaded),
        1,
        isTerminal: false,
        isFailure: false,
        isReadyForPlayback: false
    );

    public static readonly VideoStatus Processing = new(
        nameof(Processing),
        2,
        isTerminal: false,
        isFailure: false,
        isReadyForPlayback: false
    );

    public static readonly VideoStatus Transcoding = new(
        nameof(Transcoding),
        3,
        isTerminal: false,
        isFailure: false,
        isReadyForPlayback: false
    );

    public static readonly VideoStatus Finished = new(
        nameof(Finished),
        4,
        isTerminal: true,
        isFailure: false,
        isReadyForPlayback: true
    );

    public static readonly VideoStatus Error = new(
        nameof(Error),
        5,
        isTerminal: true,
        isFailure: true,
        isReadyForPlayback: false
    );

    public static readonly VideoStatus UploadFailed = new(
        nameof(UploadFailed),
        6,
        isTerminal: true,
        isFailure: true,
        isReadyForPlayback: false
    );

    public static readonly VideoStatus JitSegmenting = new(
        nameof(JitSegmenting),
        7,
        isTerminal: false,
        isFailure: false,
        isReadyForPlayback: false
    );

    public static readonly VideoStatus JitPlaylistsCreated = new(
        nameof(JitPlaylistsCreated),
        8,
        isTerminal: true,
        isFailure: false,
        isReadyForPlayback: true
    );

    public bool IsTerminal { get; }
    public bool IsFailure { get; }
    public bool IsReadyForPlayback { get; }

    private VideoStatus(
        string name,
        int value,
        bool isTerminal,
        bool isFailure,
        bool isReadyForPlayback
    )
        : base(name, value)
    {
        IsTerminal = isTerminal;
        IsFailure = isFailure;
        IsReadyForPlayback = isReadyForPlayback;
    }

    public bool CanTransitionTo(VideoStatus next)
    {
        // Same state is handled idempotently (metadata-only update)
        if (this == next)
        {
            return true;
        }

        // Finished can still advance to JitPlaylistsCreated if JIT playlists arrive later
        if (this == Finished && next == JitPlaylistsCreated)
        {
            return true;
        }

        // Otherwise, do not regress or leave terminal states
        if (IsTerminal)
        {
            return false;
        }

        return true;
    }
}
