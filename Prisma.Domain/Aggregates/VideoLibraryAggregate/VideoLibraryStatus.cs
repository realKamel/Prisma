using Ardalis.SmartEnum;

namespace Prisma.Domain.Aggregates.VideoLibraryAggregate;

public sealed class VideoLibraryStatus : SmartEnum<VideoLibraryStatus>
{
    public static readonly VideoLibraryStatus Pending = new(
        nameof(Pending),
        1,
        isTerminal: false,
        isFailure: false,
        isReady: false
    );

    public static readonly VideoLibraryStatus Active = new(
        nameof(Active),
        2,
        isTerminal: false,
        isFailure: false,
        isReady: true
    );

    public static readonly VideoLibraryStatus Failed = new(
        nameof(Failed),
        3,
        isTerminal: false,
        isFailure: true,
        isReady: false
    );

    public static readonly VideoLibraryStatus Deleted = new(
        nameof(Deleted),
        4,
        isTerminal: true,
        isFailure: false,
        isReady: false
    );

    public bool IsTerminal { get; }
    public bool IsFailure { get; }
    public bool IsReady { get; }

    private VideoLibraryStatus(
        string name,
        int value,
        bool isTerminal,
        bool isFailure,
        bool isReady
    )
        : base(name, value)
    {
        IsTerminal = isTerminal;
        IsFailure = isFailure;
        IsReady = isReady;
    }

    public bool CanTransitionTo(VideoLibraryStatus next)
    {
        if (this == next)
            return true;

        return this switch
        {
            _ when this == Pending => next == Active || next == Failed,

            _ when this == Failed => next == Pending || next == Deleted,

            _ when this == Active => next == Deleted,

            _ => false,
        };
    }
}
