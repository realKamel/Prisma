namespace Prisma.Application.Abstractions.ObjectStorage;

public sealed record FileObjectMetadata
{
    // ContentLength	Object size in bytes
    public long SizeInBytes { get; init; }

    // Headers.ContentType	Content type
    public string ContentType { get; init; } = string.Empty;

    // ETag	Object entity tag
    public string ETag { get; init; } = string.Empty;

    // LastModified	Last modification timestamp
    public DateTimeOffset LastModified { get; init; }

    // Metadata	Custom metadata attached during upload
    public Dictionary<string, string> Metadata { get; init; } = new();

    //VersionId	Object version, if supported/configured
    public string? VersionId { get; init; }
}
