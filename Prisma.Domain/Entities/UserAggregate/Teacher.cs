using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities.UserAggregate;

public class Teacher : User
{
    public string Subject { get; set; } = string.Empty;
    //public ICollection<Student> Students { get; set; } = new List<Student>();
    public TeacherStatus Status { get; set; } = TeacherStatus.Active;
    public string? SuspensionReason { get; set; }
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public ICollection<AcademicYearTeacher> AcademicYears { get; set; } = new List<AcademicYearTeacher>();
    public ICollection<Assistant> Assistants { get; set; } = new List<Assistant>();
    public TeacherLandingSettings? TeacherLandingSettings { get; set; }
    public TeacherPreferences Preferences { get; set; } = null!;
    public ICollection<TeacherStudent> TeacherStudents { get; set; } = new List<TeacherStudent>();

}