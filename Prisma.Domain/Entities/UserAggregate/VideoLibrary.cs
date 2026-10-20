using Ardalis.Result;
using Prisma.Domain.Common;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Entities.UserAggregate;

public sealed class VideoLibrary : BaseEntity
{
    private VideoLibrary() { }

    public long ProviderLibraryId { get; private set; }
    public long TrafficUsage { get; private set; }
    public long StorageUsage { get; private set; }
    public string? LibraryApiKey { get; private set; }
    public string? ReadOnlyApiKey { get; private set; }
    public string? TokenAuthKey { get; private set; }
    public string? CdnHostname { get; private set; }
    public Uri? WebhookUrl { get; private set; }
    public Uri? PullZoneUrl { get; private set; }

    //relation (teacher <-1:1=> Library)
    public Teacher Teacher { get; private set; }
    public Guid TeacherId { get; private set; }

    //relations (Library <-1:M=> Video)
    public ICollection<Video> Videos { get; private set; } = [];

    public static Result<VideoLibrary> Create(
        long providerId,
        string? libraryApiKey,
        string readOnlyApiKey,
        string tokenAuthKey,
        string cdnHostname,
        string webhookUrl,
        string pullZoneUrl
    )
    {
        var library = new VideoLibrary
        {
            ProviderLibraryId = providerId,
            LibraryApiKey = libraryApiKey,
            ReadOnlyApiKey = readOnlyApiKey,
            TokenAuthKey = tokenAuthKey,
            CdnHostname = cdnHostname,
            WebhookUrl = new Uri(webhookUrl),
            PullZoneUrl = new Uri(pullZoneUrl),
        };
        return Result.Success(library);
    }

    public void SetTeacher(Teacher teacher)
    {
        Teacher = teacher;
    }
}
