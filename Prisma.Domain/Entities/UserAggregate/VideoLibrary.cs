using Ardalis.Result;
using Prisma.Domain.Common;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Entities.UserAggregate;

public sealed class VideoLibrary : BaseEntity
{
    private VideoLibrary() { }

    //TODO: should have UniqueIdx ?
    public long ProviderLibraryId { get; private set; }
    public long TrafficUsage { get; private set; }
    public long StorageUsage { get; private set; }
    public string? LibraryApiKey { get; private set; }
    public string? ReadOnlyApiKey { get; private set; }

    // public string? TokenAuthKey { get; private set; }
    // public string? CdnHostname { get; private set; }
    public Uri? WebhookUrl { get; private set; }

    // public long? PullZoneId { get; private set; }

    //relation (teacher <-1:1=> Library)
    // public Teacher Teacher { get; private set; }
    public Guid TeacherId { get; private set; }

    public ProvisioningStatus ProvisioningStatus { get; set; }

    public EncodingTier EncodingTier { get; set; }

    //relations (Library <-1:M=> Video)
    public ICollection<Video> Videos { get; private set; } = [];

    public static Result<VideoLibrary> Create(Guid teacherId)
    {
        var library = new VideoLibrary
        {
            ProvisioningStatus = ProvisioningStatus.Pending,
            TeacherId = teacherId,
        };
        return Result.Success(library);
    }

    public void AttachToTeacher(Guid teacherId)
    {
        TeacherId = teacherId;
    }

    public void MarkLibraryAsActive(
        long providerId,
        string? libraryApiKey,
        string readOnlyApiKey,
        string webhookUrl
    )
    {
        ProviderLibraryId = providerId;
        LibraryApiKey = libraryApiKey;
        ReadOnlyApiKey = readOnlyApiKey;
        // TokenAuthKey = tokenAuthKey;
        // CdnHostname = cdnHostname;
        WebhookUrl = new Uri(webhookUrl);
        // PullZoneId = pullZoneId;
        ProvisioningStatus = ProvisioningStatus.Active;
    }

    public void MarkAsFailed()
    {
        ProvisioningStatus = ProvisioningStatus.Failed;
    }

    public static string GenerateLibraryName(Guid teacherId)
    {
        return $"wasla-teacher-{teacherId:N}";
    }
}
