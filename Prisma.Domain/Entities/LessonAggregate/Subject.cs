using Prisma.Domain.Common;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Entities.LessonAggregate;

public class Subject : BaseEntity
{
    public string Title { get; set; }
    public string Descripation { get; set; }
    public ICollection<AcademicYear> AcademicYears { get; set; } = [];
    public ICollection<Student> Students { get; set; } = [];
    public ICollection<Teacher> Teachers { get; set; } = [];
}
