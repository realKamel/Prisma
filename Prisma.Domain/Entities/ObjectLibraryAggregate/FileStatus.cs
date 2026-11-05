using Ardalis.SmartEnum;

namespace Prisma.Domain.Entities.ObjectLibraryAggregate;

public sealed class FileStatus : SmartEnum<FileStatus>
{
    public static readonly FileStatus PendingUpload = new(nameof(PendingUpload), 1);

    public static readonly FileStatus PendingScan = new(nameof(PendingScan), 2);

    public static readonly FileStatus Scanning = new(nameof(Scanning), 3);

    public static readonly FileStatus Available = new(nameof(Available), 4);

    public static readonly FileStatus Rejected = new(nameof(Rejected), 5);

    public static readonly FileStatus ScanFailed = new(nameof(ScanFailed), 6);

    public bool CanBeDownloaded => this == Available;

    public bool CanStartScanning => this == PendingScan;

    public bool IsTerminal => this == Available || this == Rejected;

    private FileStatus(string name, int value)
        : base(name, value) { }

    public bool CanTransitionTo(FileStatus target) =>
        this switch
        {
            var s when s == PendingUpload => target == PendingScan,
            var s when s == PendingScan => target == Scanning,
            var s when s == Scanning => target == Available
                || target == Rejected
                || target == ScanFailed,
            var s when s == ScanFailed => target == PendingScan,
            _ => false,
        };
}
