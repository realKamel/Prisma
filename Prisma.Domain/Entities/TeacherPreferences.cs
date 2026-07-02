using System;
using System.Collections.Generic;
using System.Text;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities;

public class TeacherPreferences
{
    public Guid TeacherId { get; private set; }
    public Teacher Teacher { get; private set; } = null!;

    public AccentColor AccentColor { get; private set; } = AccentColor.Purple;

    private TeacherPreferences() { } // EF

    public static TeacherPreferences CreateDefault(Guid teacherId)
    {
        return new TeacherPreferences
        {
            TeacherId = teacherId,
            AccentColor = AccentColor.Purple
        };
    }

    public void UpdateAccentColor(AccentColor accentColor)
    {
        AccentColor = accentColor;
    }


}
