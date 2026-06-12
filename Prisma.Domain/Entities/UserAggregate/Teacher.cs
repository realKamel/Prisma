using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Entities.UserAggregate;

public class Teacher : User
{
    public string Subject { get; set; } = string.Empty;
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public ICollection<AcademicYear> AcademicYears { get; set; } = new List<AcademicYear>();
    public ICollection<Assistant> Assistants { get; set; } = new List<Assistant>();
    public TeacherLandingSettings? TeacherLandingSettings { get; set; }
}