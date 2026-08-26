using System.ComponentModel.DataAnnotations;

namespace Prisma.Infrastructure.Services.Auth;

public sealed class JwtSettingsOptions
{
    public const string SectionName = "JwtSettings";

    [Required]
    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ExpiryInMinutes { get; set; } = 15;

    public int RefreshTokenExpiryInDays { get; set; } = 7;
}
