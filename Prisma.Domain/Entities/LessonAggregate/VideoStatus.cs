using Ardalis.SmartEnum;

namespace Prisma.Domain.Entities.LessonAggregate;

public sealed class VideoStatus : SmartEnum<VideoStatus>
{
    // Failure states are reachable from any live state and are never left, so they sit
    // outside the happy-path progression.
    private const int FailureRank = int.MaxValue;
    private const int DeletedRank = int.MaxValue;

    public static readonly VideoStatus Created = new(
        nameof(Created),
        0,
        rank: 0,
        isTerminal: false,
        isFailure: false,
        isReadyForPlayback: false
    );

    public static readonly VideoStatus Uploaded = new(
        nameof(Uploaded),
        1,
        rank: 1,
        isTerminal: false,
        isFailure: false,
        isReadyForPlayback: false
    );

    public static readonly VideoStatus Processing = new(
        nameof(Processing),
        2,
        rank: 2,
        isTerminal: false,
        isFailure: false,
        isReadyForPlayback: false
    );

    public static readonly VideoStatus Transcoding = new(
        nameof(Transcoding),
        3,
        rank: 3,
        isTerminal: false,
        isFailure: false,
        isReadyForPlayback: false
    );

    public static readonly VideoStatus JitSegmenting = new(
        nameof(JitSegmenting),
        7,
        rank: 4,
        isTerminal: false,
        isFailure: false,
        isReadyForPlayback: false
    );

    public static readonly VideoStatus Finished = new(
        nameof(Finished),
        4,
        rank: 5,
        isTerminal: true,
        isFailure: false,
        isReadyForPlayback: true
    );

    public static readonly VideoStatus JitPlaylistsCreated = new(
        nameof(JitPlaylistsCreated),
        8,
        rank: 6,
        isTerminal: true,
        isFailure: false,
        isReadyForPlayback: true
    );

    public static readonly VideoStatus Error = new(
        nameof(Error),
        5,
        rank: FailureRank,
        isTerminal: true,
        isFailure: true,
        isReadyForPlayback: false
    );

    public static readonly VideoStatus UploadFailed = new(
        nameof(UploadFailed),
        6,
        rank: FailureRank,
        isTerminal: true,
        isFailure: true,
        isReadyForPlayback: false
    );

    public static readonly VideoStatus Deleted = new(
        nameof(Deleted),
        9,
        rank: DeletedRank,
        isTerminal: true,
        isFailure: false,
        isReadyForPlayback: false
    );

    public bool IsTerminal { get; }
    public bool IsFailure { get; }
    public bool IsReadyForPlayback { get; }

    /// <summary>
    /// Position in the provider's happy-path progression. Used to reject out-of-order
    /// transitions while tolerating skipped ones (a lost webhook delivery).
    /// </summary>
    public int Rank { get; }

    private VideoStatus(
        string name,
        int value,
        int rank,
        bool isTerminal,
        bool isFailure,
        bool isReadyForPlayback
    )
        : base(name, value)
    {
        Rank = rank;
        IsTerminal = isTerminal;
        IsFailure = isFailure;
        IsReadyForPlayback = isReadyForPlayback;
    }

    public bool CanTransitionTo(VideoStatus next)
    {
        ArgumentNullException.ThrowIfNull(next);

        // Same state is handled idempotently (metadata-only update).
        if (this == next)
        {
            return true;
        }

        // A provider-side delete is authoritative at any point in the lifecycle.
        if (next == Deleted)
        {
            return true;
        }

        // Terminal states are never left for anything other than deletion.
        if (IsTerminal)
        {
            return false;
        }

        // Any live video can fail, including when the failure is the first event we see.
        if (next.IsFailure)
        {
            return true;
        }

        // Otherwise only forward progress is allowed, so regressions are rejected while
        // jumps (Created -> Finished) still work when deliveries were lost.
        return next.Rank > Rank;
    }
}
