namespace Prisma.Infrastructure.Persistence;

public sealed class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";
    public string DefaultSqlConnection { get; set; } = string.Empty;
    public string Valkey { get; set; } = string.Empty;
}
