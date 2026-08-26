namespace Prisma.Infrastructure.Services.StorageService;

public sealed class ObjectStorageOptions
{
    public const string SectionName = "ObjectStorage";

    //"ServiceUrl": "",
    public string ServiceUrl { get; set; } = string.Empty;

    //"AccessKey": "",
    public string AccessKey { get; set; } = string.Empty;

    //"SecretKey": "",
    public string SecretKey { get; set; } = string.Empty;

    //"BucketName": "prisma-bucket",
    public string BucketName { get; set; } = string.Empty;

    //"PublicPrefixes": ["lessons/thumbnails" ],
    public List<string> PublicPrefixes { get; set; } = [];

    //"ForcePathStyle": true
    public bool ForcePathStyle { get; set; } = true;
}
