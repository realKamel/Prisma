using System.ComponentModel.DataAnnotations.Schema;

namespace Prisma.Domain.ValueObjects.UserDomain;

/// <summary>
/// The teacher's public contact links. Stored already normalised (https URLs, WhatsApp as digits).
/// Mapped by EF as a complex type, so the columns live on the teacher row.
/// </summary>
[ComplexType]
public sealed record ContactLinks
{
    public string? Whatsapp { get; init; }
    public string? Facebook { get; init; }
    public string? Instagram { get; init; }
    public string? Youtube { get; init; }
    public string? Telegram { get; init; }
    public string? Tiktok { get; init; }
    public string? Website { get; init; }
}
