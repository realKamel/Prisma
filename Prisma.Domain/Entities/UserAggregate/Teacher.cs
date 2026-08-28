using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities.UserAggregate;

public class Teacher : User
{
    public string Subject { get; set; } = string.Empty;
    public string? TeacherAvatarUrl { get; set; }

    //public ICollection<Student> Students { get; set; } = new List<Student>();
    public TeacherStatus Status { get; set; } = TeacherStatus.Active;
    public string? SuspensionReason { get; set; }
    public ICollection<Lesson> Lessons { get; set; } = [];
    public ICollection<AcademicYearTeacher> AcademicYears { get; set; } = [];
    public ICollection<Assistant> Assistants { get; set; } = [];
    public TeacherLandingSettings? TeacherLandingSettings { get; set; }
    public TeacherPreferences Preferences { get; set; } = null!;

    public ICollection<TeacherStudent> TeacherStudents { get; set; } = [];
    //public ICollection<Student> Students { get; set; } = new List<Student>();
}
