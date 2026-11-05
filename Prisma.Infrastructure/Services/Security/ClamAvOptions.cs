namespace Prisma.Infrastructure.Services.Security;

public class ClamAvOptions
{
    public const string SectionName = "ClamAv";
    public string Host { get; set; } = "clamav";
    public int Port { get; set; } = 3310;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(120);

    // Bytes: 104,857,600 bytes
    // Kilobytes (KB): 102,400 KB
    // Megabytes (MB): 100 MB
    public long MaxFileSizeInBytes { get; set; } = 100 * 1024 * 1024;
}
