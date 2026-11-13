using Ardalis.Result;

namespace Prisma.Domain.Entities.UserAggregate;

public sealed record TenantSetting
{
    private TenantSetting() { }

    public Guid TenantId { get; private set; } // e.g., "tenant-acme-corp"
    public string LibraryId { get; private set; }
    public string LibraryApiKey { get; private set; }
    public string ReadOnlyLibraryApiKey { get; private set; }
    public Uri PullZoneUrl { get; private set; }

    public static Result<TenantSetting> Create(
        Guid tenantId,
        string libraryId,
        string apiKey,
        string readonlyApiKey,
        Uri pullZoneUrl
    )
    {
        var validationErrors = new List<ValidationError>();

        if (tenantId == Guid.Empty)
        {
            validationErrors.Add(
                new ValidationError(nameof(TenantId), "Tenant ID cannot be empty.")
            );
        }

        if (string.IsNullOrWhiteSpace(libraryId))
        {
            validationErrors.Add(
                new ValidationError(nameof(LibraryId), "Library ID cannot be null or whitespace.")
            );
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            validationErrors.Add(
                new ValidationError(nameof(apiKey), "API Key cannot be null or whitespace.")
            );
        }

        if (pullZoneUrl is null)
        {
            validationErrors.Add(
                new ValidationError(nameof(PullZoneUrl), "Pull Zone URL cannot be null.")
            );
        }

        if (pullZoneUrl is not null && !pullZoneUrl.IsAbsoluteUri)
        {
            validationErrors.Add(
                new ValidationError(nameof(PullZoneUrl), "Pull Zone URL must be an absolute URI.")
            );
        }

        if (validationErrors.Count > 0)
        {
            return Result.Invalid(validationErrors);
        }

        var tenantSetting = new TenantSetting
        {
            TenantId = tenantId,
            LibraryId = libraryId,
            LibraryApiKey = apiKey,
            ReadOnlyLibraryApiKey = readonlyApiKey,
            PullZoneUrl = pullZoneUrl,
        };

        return Result<TenantSetting>.Success(tenantSetting);
    }
}
