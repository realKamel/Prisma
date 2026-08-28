using Prisma.Domain.Common;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities;

public class TeacherPreferences : IAuditable, IEntity<Guid>
{
    public Guid Id { get; set; }
    public Teacher Teacher { get; set; } = null!;

    public AccentColor AccentColor { get; private set; } = AccentColor.Purple;

    private TeacherPreferences() { } // EF

    public static TeacherPreferences CreateDefault(Guid teacherId)
    {
        return new TeacherPreferences { Id = teacherId, AccentColor = AccentColor.Purple };
    }

    public void UpdateAccentColor(AccentColor accentColor)
    {
        AccentColor = accentColor;
    }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public Guid? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
}
