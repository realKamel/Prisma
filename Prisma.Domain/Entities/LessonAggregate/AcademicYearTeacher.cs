using Prisma.Domain.Common;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Entities.LessonAggregate;

public class AcademicYearTeacher : BaseEntity
{
    public AcademicYear AcademicYear { get; set; }
    public int AcademicYearId { get; set; }

    public Teacher Teacher { get; set; }
    public Guid TeacherId { get; set; }
}
