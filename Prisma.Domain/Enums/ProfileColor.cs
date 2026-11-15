using System.Text.Json.Serialization;

namespace Prisma.Domain.Enums;

/// <summary>
/// Gradient presets for the public teacher page. Names match the Angular `ProfileColor` union.
/// </summary>
public enum ProfileColor
{
    Ocean,
    Midnight,
    Violet,
    Teal,
    Emerald,
    Graphite,
    Rose,
    Sunset,
}
