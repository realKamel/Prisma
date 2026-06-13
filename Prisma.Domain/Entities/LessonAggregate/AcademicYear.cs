using Prisma.Domain.Common;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Entities.LessonAggregate;

public class AcademicYear : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}