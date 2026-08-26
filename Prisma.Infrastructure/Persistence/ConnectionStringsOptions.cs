namespace Prisma.Infrastructure.Persistence;

public sealed class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";
    public string PostgresConnectionString { get; set; } = string.Empty;
    public string ValkeyConnectionString { get; set; } = string.Empty;
}
