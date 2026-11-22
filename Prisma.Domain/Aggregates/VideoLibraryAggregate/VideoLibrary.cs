using Ardalis.Result;
using Prisma.Domain.Common;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Aggregates.VideoLibraryAggregate;

public sealed class VideoLibrary : BaseEntity
{
    private VideoLibrary() { }

    //TODO: should have UniqueIdx ?
    public long? ProviderLibraryId { get; private set; }
    public long TrafficUsage { get; private set; }
    public long StorageUsage { get; private set; }
    public string? LibraryApiKey { get; private set; }
    public string? ReadOnlyApiKey { get; private set; }

    // public string? TokenAuthKey { get; private set; }
    // public string? CdnHostname { get; private set; }
    // public Uri? WebhookUrl { get; private set; }

    // public long? PullZoneId { get; private set; }

    //relation (teacher <-1:1=> Library)
    // public Teacher Teacher { get; private set; }
    public Guid TeacherId { get; private set; }

    public VideoLibraryStatus Status { get; private set; } = VideoLibraryStatus.Pending;

    public EncodingTier EncodingTier { get; set; }

    //relations (Library <-1:M=> Video)
    public ICollection<Video> Videos { get; private set; } = [];

    public static Result<VideoLibrary> Create(Guid teacherId)
    {
        var library = new VideoLibrary
        {
            Status = VideoLibraryStatus.Pending,
            TeacherId = teacherId,
        };
        return Result.Success(library);
    }

    public void AttachToTeacher(Guid teacherId)
    {
        TeacherId = teacherId;
    }

    public void ConfigureProviderCredentials(
        long providerId,
        string libraryApiKey,
        string readOnlyApiKey
    )
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(providerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(libraryApiKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(readOnlyApiKey);

        if (ProviderLibraryId.HasValue)
        {
            throw new InvalidOperationException("Provider library is already configured.");
        }

        ProviderLibraryId = providerId;
        LibraryApiKey = libraryApiKey;
        ReadOnlyApiKey = readOnlyApiKey;
    }

    public void MarkAsActive()
    {
        EnsureProviderCredentialsConfigured();

        ChangeStatus(VideoLibraryStatus.Active);
    }

    public void MarkAsFailed()
    {
        ChangeStatus(VideoLibraryStatus.Failed);
    }

    public void MarkAsDeleted()
    {
        ChangeStatus(VideoLibraryStatus.Deleted);
    }

    public void RetryProvisioning()
    {
        ChangeStatus(VideoLibraryStatus.Pending);
    }

    private void ChangeStatus(VideoLibraryStatus newStatus)
    {
        if (!Status.CanTransitionTo(newStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition library from {Status} to {newStatus}."
            );
        }

        Status = newStatus;
    }

    public static string GenerateLibraryName(Guid teacherId)
    {
        return $"wasla-teacher-{teacherId:N}";
    }

    private void EnsureProviderCredentialsConfigured()
    {
        if (
            ProviderLibraryId is null
            || string.IsNullOrWhiteSpace(LibraryApiKey)
            || string.IsNullOrWhiteSpace(ReadOnlyApiKey)
        )
        {
            throw new InvalidOperationException(
                "Provider credentials must be configured before activation."
            );
        }
    }
}
