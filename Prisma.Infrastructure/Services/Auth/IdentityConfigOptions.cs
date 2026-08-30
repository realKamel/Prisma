namespace Prisma.Infrastructure.Services.Auth;

public class IdentityConfigOptions
{
    public const string SectionName = "IdentitySeed";
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string AdminPhone { get; set; } = string.Empty;
}
